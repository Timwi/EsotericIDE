using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    partial class Funciton
    {
        private enum connectorType { None, Input, Output }
        private enum lineType { None, Single, Double }
        private enum nodeType { Declaration, Call, Literal, Comment, TJunction, CrossJunction, End }
        private enum direction { Up = 0, Right = 1, Down = 2, Left = 3 }

        private ExecutionEnvironment compileAndAnalyse(IEnumerable<Tuple<string, string>> pathsAndSources, BigInteger? stdin)
        {
            unparsedProgram program = null;
            unparsedDeclaration functionToAnalyse = null;
            var callToDecl = new Dictionary<node, unparsedFunctionDeclaration>();
            var declarationsByName = new Dictionary<string, unparsedFunctionDeclaration>();

            foreach (var tuple in pathsAndSources)
            {
                var sourceFile = tuple.Item1;
                var sourceText = tuple.Item2;

                // Turn into array of characters
                var lines = (sourceText.Replace("\r", "") + "\n\n").Split('\n');
                if (lines.Length == 0)
                    continue;

                var longestLine = lines.Max(l => l.Length);
                if (longestLine == 0)
                    continue;

                var source = new sourceAsChars { Chars = lines.Select(l => l.PadRight(longestLine).ToCharArray()).ToArray(), SourceText = sourceText };

                // Detect some common problems
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        if (x < source.Width - 1 && source.RightLine(x, y) != lineType.None && source.LeftLine(x + 1, y) != lineType.None && source.RightLine(x, y) != source.LeftLine(x + 1, y))
                            FuncitonHelper.ThrowParseError("Single line cannot suddenly switch to double line.", x + 1, y, sourceFile);
                        if (y < source.Height - 1 && source.BottomLine(x, y) != lineType.None && source.TopLine(x, y + 1) != lineType.None && source.BottomLine(x, y) != source.TopLine(x, y + 1))
                            FuncitonHelper.ThrowParseError("Single line cannot suddenly switch to double line.", x, y + 1, sourceFile);
                    }
                }

                // Find boxes and their outgoing edges
                var nodes = new List<node>();
                var unfinishedEdges = new List<unfinishedEdge>();
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        // Start finding a box here if this is a top-left corner of a box
                        if (source.TopLine(x, y) != lineType.None || source.LeftLine(x, y) != lineType.None || source.RightLine(x, y) == lineType.None || source.BottomLine(x, y) == lineType.None)
                            continue;

                        // Find width of box by walking along top edge
                        var top = source.RightLine(x, y);
                        var index = x + 1;
                        while (index < source.Width && source.RightLine(index, y) == top)
                            index++;
                        if (index == source.Width || source.BottomLine(index, y) == lineType.None || source.TopLine(index, y) != lineType.None || source.RightLine(index, y) != lineType.None)
                            continue;
                        var width = index - x;

                        // Find height of box by walking along left edge
                        var left = source.BottomLine(x, y);
                        index = y + 1;
                        while (index < source.Height && source.BottomLine(x, index) == left)
                            index++;
                        if (index == source.Height || source.RightLine(x, index) == lineType.None || source.LeftLine(x, index) != lineType.None || source.BottomLine(x, index) != lineType.None)
                            continue;
                        var height = index - y;

                        // Verify the bottom edge
                        var bottom = source.RightLine(x, y + height);
                        index = x + 1;
                        while (index < source.Width && source.RightLine(index, y + height) == bottom)
                            index++;
                        if (index == source.Width || source.TopLine(index, y + height) == lineType.None || source.BottomLine(index, y + height) != lineType.None || source.RightLine(index, y + height) != lineType.None)
                            continue;
                        if (index - x != width)
                            continue;

                        // Verify the right edge
                        var right = source.BottomLine(x + width, y);
                        index = y + 1;
                        while (index < source.Height && source.BottomLine(x + width, index) == right)
                            index++;
                        if (index == source.Height || source.LeftLine(x + width, index) == lineType.None || source.RightLine(x + width, index) != lineType.None || source.BottomLine(x + width, index) != lineType.None)
                            continue;
                        if (index - y != height)
                            continue;

                        // Determine type of box
                        nodeType type;
                        var edgeTypes = new[] { left, top, right, bottom };
                        if (edgeTypes.All(e => e == lineType.Single))
                            // Not actually a box but a NAND square
                            continue;
                        else if (edgeTypes.All(e => e == lineType.Double))
                            type = nodeType.Literal;
                        else if (Enumerable.Range(0, 4).All(i => edgeTypes[i] != edgeTypes[(i + 1) % 4]))
                            type = nodeType.Declaration;
                        else if (edgeTypes.Count(e => e == lineType.Double) == 2)
                            type = nodeType.Call;
                        else
                        {
                            FuncitonHelper.ThrowParseError("Unrecognised box type.", x, y, sourceFile);
                            continue;
                        }

                        // Right now, “type” is “Literal” if it is a double-lined box, but it could be a Comment too,
                        // so don’t create the box yet. When we encounter an outgoing edge, we’ll know it’s a literal.
                        node box = null;
                        Func<node> getBox = () => box ?? (box = new node(x, y, width, height, type));

                        // Search for outgoing edges
                        unfinishedEdge topEdge = null, rightEdge = null, bottomEdge = null, leftEdge = null;
                        for (int i = x + 1; i < x + width; i++)
                        {
                            if (source.TopLine(i, y) == lineType.Double)
                                FuncitonHelper.ThrowParseError("Box has outgoing double edge.", i, y, sourceFile);
                            else if (source.TopLine(i, y) == lineType.Single)
                            {
                                if (topEdge != null)
                                    FuncitonHelper.ThrowParseError("Box has duplicate outgoing edge along the top.", i, y, sourceFile);
                                topEdge = new unfinishedEdge { StartNode = getBox(), DirectionFromStartNode = direction.Up, DirectionGoingTo = direction.Up, StartX = i, StartY = y, EndX = i, EndY = y };
                            }

                            if (source.BottomLine(i, y + height) == lineType.Double)
                                FuncitonHelper.ThrowParseError("Box has outgoing double edge.", i, y + height, sourceFile);
                            else if (source.BottomLine(i, y + height) == lineType.Single)
                            {
                                if (bottomEdge != null)
                                    FuncitonHelper.ThrowParseError("Box has duplicate outgoing edge along the bottom.", i, y + height, sourceFile);
                                bottomEdge = new unfinishedEdge { StartNode = getBox(), DirectionFromStartNode = direction.Down, DirectionGoingTo = direction.Down, StartX = i, StartY = y + height, EndX = i, EndY = y + height };
                            }
                        }
                        for (int i = y + 1; i < y + height; i++)
                        {
                            if (source.LeftLine(x, i) == lineType.Double)
                                FuncitonHelper.ThrowParseError("Box has outgoing double edge.", x, i, sourceFile);
                            else if (source.LeftLine(x, i) == lineType.Single)
                            {
                                if (leftEdge != null)
                                    FuncitonHelper.ThrowParseError("Box has duplicate outgoing edge along the left.", x, i, sourceFile);
                                leftEdge = new unfinishedEdge { StartNode = getBox(), DirectionFromStartNode = direction.Left, DirectionGoingTo = direction.Left, StartX = x, StartY = i, EndX = x, EndY = i };
                            }

                            if (source.RightLine(x + width, i) == lineType.Double)
                                FuncitonHelper.ThrowParseError("Box has outgoing double edge.", x + width, i, sourceFile);
                            else if (source.RightLine(x + width, i) == lineType.Single)
                            {
                                if (rightEdge != null)
                                    FuncitonHelper.ThrowParseError("Box has duplicate outgoing edge along the right.", x + width, i, sourceFile);
                                rightEdge = new unfinishedEdge { StartNode = getBox(), DirectionFromStartNode = direction.Right, DirectionGoingTo = direction.Right, StartX = x + width, StartY = i, EndX = x + width, EndY = i };
                            }
                        }

                        // If box is still null, then it has no outgoing edges.
                        if (box == null)
                        {
                            if (type == nodeType.Literal)
                                type = nodeType.Comment;
                            else
                                FuncitonHelper.ThrowParseError("Box without outgoing edges not allowed unless it has only double-lined edges (making it a comment).", x, y, sourceFile);
                        }

                        // If it’s a comment, kill its contents so that it can contain boxes if it wants to.
                        if (type == nodeType.Comment)
                        {
                            for (int yy = y; yy <= y + height; yy++)
                                for (int xx = x; xx <= x + width; xx++)
                                    source.Chars[yy][xx] = ' ';
                        }
                        else
                        {
                            nodes.Add(getBox());
                            unfinishedEdges.AddRange(new[] { topEdge, rightEdge, bottomEdge, leftEdge }.Where(e => e != null));
                        }
                    }
                }

                // Add T-junctions and cross-junctions (but not loose ends yet), and also complain about any stray characters
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        if (source.Chars[y][x] == ' ')
                            continue;
                        // ignore boxes
                        if (nodes.Any(b => b.X <= x && b.X + b.Width >= x && b.Y <= y && b.Y + b.Height >= y))
                            continue;
                        if ((!source.AnyLine(x, y) || source.TopLine(x, y) == lineType.Double || source.LeftLine(x, y) == lineType.Double || source.BottomLine(x, y) == lineType.Double || source.RightLine(x, y) == lineType.Double))
                            FuncitonHelper.ThrowParseError("Stray character: " + source.Chars[y][x], x, y, sourceFile);

                        var singleLines = new[] { source.TopLine(x, y), source.RightLine(x, y), source.BottomLine(x, y), source.LeftLine(x, y) }.Select(line => line == lineType.Single).ToArray();
                        var count = singleLines.Count(sl => sl);
                        if (count < 3)
                            continue;

                        var nodetype = count == 4 ? nodeType.CrossJunction : nodeType.TJunction;
                        var node = new node(x, y, 0, 0, nodetype);
                        nodes.Add(node);
                        for (int i = 0; i < 4; i++)
                            if (singleLines[i])
                                unfinishedEdges.Add(new unfinishedEdge { StartNode = node, DirectionFromStartNode = (direction) i, StartX = x, StartY = y, EndX = x, EndY = y, DirectionGoingTo = (direction) i });
                    }
                }

                // Parse the connections between nodes and discover all the loose ends
                var visited = new bool[source.Chars.Length][];
                for (int i = visited.Length - 1; i >= 0; i--)
                    visited[i] = new bool[source.Chars[0].Length];
                var edges = new List<edge>();
                while (unfinishedEdges.Count > 0)
                {
                    var edge = unfinishedEdges[0];
                    int x = edge.EndX, y = edge.EndY;
                    lineType connector;
                    switch (edge.DirectionGoingTo)
                    {
                        case direction.Up:
                            y--;
                            connector = source.BottomLine(x, y);
                            break;
                        case direction.Left:
                            x--;
                            connector = source.RightLine(x, y);
                            break;
                        case direction.Down:
                            y++;
                            connector = source.TopLine(x, y);
                            break;
                        case direction.Right:
                            x++;
                            connector = source.LeftLine(x, y);
                            break;
                        default:
                            FuncitonHelper.ThrowParseError("The parser encountered an internal error.", x, y, sourceFile);
                            continue;
                    }
                    if (y >= 0 && y < visited.Length && x >= 0 && x < visited[y].Length)
                        visited[y][x] = true;
                    switch (connector)
                    {
                        case lineType.None:
                            // We encountered a loose end
                            unfinishedEdges.RemoveAt(0);
                            var node = new node(edge.EndX, edge.EndY, 0, 0, nodeType.End);
                            edges.Add(new edge(edge.StartNode, edge.DirectionFromStartNode, node, opposite(edge.DirectionGoingTo), edge.StartX, edge.StartY, edge.EndX, edge.EndY));
                            nodes.Add(node);
                            break;

                        case lineType.Single:
                            // Check whether this edge connects to any other edge
                            var otherEdge = unfinishedEdges.FirstOrDefault(ue => ue.EndX == x && ue.EndY == y && ue.DirectionGoingTo == opposite(edge.DirectionGoingTo));
                            if (otherEdge != null)
                            {
                                unfinishedEdges.RemoveAt(0);
                                unfinishedEdges.Remove(otherEdge);
                                edges.Add(new edge(edge.StartNode, edge.DirectionFromStartNode, otherEdge.StartNode, otherEdge.DirectionFromStartNode, edge.StartX, edge.StartY, x, y));
                                break;
                            }
                            // We can now assume this is not a junction, so just check which direction it’s going
                            edge.DirectionGoingTo =
                                edge.DirectionGoingTo != direction.Down && source.TopLine(x, y) == lineType.Single ? direction.Up :
                                edge.DirectionGoingTo != direction.Up && source.BottomLine(x, y) == lineType.Single ? direction.Down :
                                edge.DirectionGoingTo != direction.Left && source.RightLine(x, y) == lineType.Single ? direction.Right :
                                edge.DirectionGoingTo != direction.Right && source.LeftLine(x, y) == lineType.Single ? direction.Left :
                                FuncitonHelper.ThrowParseError<direction>("The parser encountered an internal error.", x, y, sourceFile);
                            edge.EndX = x;
                            edge.EndY = y;
                            break;

                        case lineType.Double:
                        default:
                            FuncitonHelper.ThrowParseError("Unexpected double line.", x, y, sourceFile);
                            continue;
                    }
                }

                // Complain about any extraneous characters anywhere
                for (int y = 0; y < source.Height; y++)
                    for (int x = 0; x < source.Width; x++)
                        if (source.Chars[y][x] != ' ' && !visited[y][x] && !nodes.Any(b => b.X <= x && b.X + b.Width >= x && b.Y <= y && b.Y + b.Height >= y))
                            FuncitonHelper.ThrowParseError("Stray line not connected to any program or function.", x, y, sourceFile);

                // Collect everything that is connected to each declaration node
                List<node> collectedNodes;
                List<edge> collectedEdges;
                var declarations = new List<unparsedFunctionDeclaration>();
                while (true)
                {
                    var declaration = nodes.FirstOrDefault(n => n.Type == nodeType.Declaration);
                    if (declaration == null)
                        break;
                    collectAllConnected(nodes, edges, declaration, out collectedNodes, out collectedEdges);
                    var unparsedFunction = new unparsedFunctionDeclaration(collectedNodes, collectedEdges, source);
                    declarations.Add(unparsedFunction);
                    if (unparsedFunction.DeclarationName == _settings.AnalyzeFunction)
                        functionToAnalyse = unparsedFunction;
                }

                // If there is anything left, it must be the program. There must be exactly one output
                var outputs = nodes.Where(n => n.Type == nodeType.End).ToList();
                if (outputs.Count > 1)
                    FuncitonHelper.ThrowParseError("Cannot have more than one program output.", outputs[1].X, outputs[1].Y, sourceFile);
                else if (outputs.Count == 1)
                {
                    if (program != null)
                        FuncitonHelper.ThrowParseError("Cannot have more than one program.", outputs[0].X, outputs[0].Y, sourceFile);
                    collectAllConnected(nodes, edges, outputs[0], out collectedNodes, out collectedEdges);
                    program = new unparsedProgram(collectedNodes, collectedEdges, source);
                }

                // If there is *still* anything left (other than comments), it’s an error
                var strayNode = nodes.FirstOrDefault(n => n.Type != nodeType.Comment);
                if (strayNode != null)
                    FuncitonHelper.ThrowParseError("Stray node unconnected to any declaration or program.", strayNode.X, strayNode.Y, sourceFile);
                var strayEdge = edges.FirstOrDefault();
                if (strayEdge != null)
                    FuncitonHelper.ThrowParseError("Stray edge unconnected to any declaration or program.", strayEdge.StartX, strayEdge.StartY, sourceFile);

                // Check that all function names are unique
                var privateDeclarationsByName = new Dictionary<string, unparsedFunctionDeclaration>();
                foreach (var decl in declarations)
                {
                    if (declarationsByName.ContainsKey(decl.DeclarationName) || privateDeclarationsByName.ContainsKey(decl.DeclarationName))
                        FuncitonHelper.ThrowParseError("Duplicate function declaration: ‘{0}’.".Fmt(decl.DeclarationName), decl.DeclarationNode.X, decl.DeclarationNode.Y, sourceFile);
                    (decl.DeclarationIsPrivate ? privateDeclarationsByName : declarationsByName)[decl.DeclarationName] = decl;
                }

                // Associate all the call nodes that call a private function with the relevant declaration
                IEnumerable<unparsedDeclaration> decls = declarations;
                if (outputs.Count == 1)
                    decls = decls.Concat(new unparsedDeclaration[] { program });
                foreach (var decl in decls)
                    foreach (var node in decl.Nodes.Where(n => n.Type == nodeType.Call))
                    {
                        unparsedFunctionDeclaration ufd;
                        if (privateDeclarationsByName.TryGetValue(node.GetContent(source), out ufd))
                            callToDecl[node] = ufd;
                    }
            }

            if (program == null)
                FuncitonHelper.ThrowParseError("Source files do not contain a program (program must have an output).", 0, 0);

            var functions = new Dictionary<unparsedDeclaration, FuncitonFunction>();

            if (_settings.AnalyzeFunction == null)
                return new executionEnvironment(program.Parse(declarationsByName, callToDecl, functions, stdin), _settings.TraceFunctions);

            return new analysisExecutionEnvironment(
                _settings.AnalyzeFunction == "" ? program.Parse(declarationsByName, callToDecl, functions, stdin).Analyse() :
                functionToAnalyse == null ? "No such function: “{0}”.{1}".Fmt(_settings.AnalyzeFunction, Environment.NewLine) :
                functionToAnalyse.Parse(declarationsByName, callToDecl, functions, stdin).Analyse()
            );
        }

        private static void collectAllConnected(List<node> nodes, List<edge> edges, node initialNode, out List<node> outNodes, out List<edge> outEdges)
        {
            var theseNodes = new List<node> { initialNode };
            var theseEdges = new List<edge>();
            nodes.Remove(initialNode);

            while (true)
            {
                var edge = edges.FirstOrDefault(e => theseNodes.Contains(e.StartNode));
                if (edge != null)
                {
                    edges.Remove(edge);
                    theseEdges.Add(edge);
                    nodes.Remove(edge.EndNode);
                    if (!theseNodes.Contains(edge.EndNode))
                        theseNodes.Add(edge.EndNode);
                }
                else
                {
                    edge = edges.FirstOrDefault(e => theseNodes.Contains(e.EndNode));
                    if (edge == null)
                        break;
                    edges.Remove(edge);
                    theseEdges.Add(edge);
                    nodes.Remove(edge.StartNode);
                    if (!theseNodes.Contains(edge.StartNode))
                        theseNodes.Add(edge.StartNode);
                }
            }
            outNodes = theseNodes;
            outEdges = theseEdges;
        }

        private sealed class sourceAsChars
        {
            public char[][] Chars;
            public string SourceText;

            public lineType TopLine(int x, int y)
            {
                return
                    y < 0 || y >= Chars.Length || x < 0 || x >= Chars[y].Length ? lineType.None :
                    "│└┘├┤┴╛╘╡╧┼╞╪".Contains(Chars[y][x]) ? lineType.Single :
                    "║╚╝╠╣╩╜╙╢╨╬╟╫".Contains(Chars[y][x]) ? lineType.Double : lineType.None;
            }
            public lineType LeftLine(int x, int y)
            {
                return
                    y < 0 || y >= Chars.Length || x < 0 || x >= Chars[y].Length ? lineType.None :
                    "─┐┘┤┬┴╜╖╢╨╥╫┼".Contains(Chars[y][x]) ? lineType.Single :
                    "═╗╝╣╦╩╛╕╡╧╤╪╬".Contains(Chars[y][x]) ? lineType.Double : lineType.None;
            }
            public lineType RightLine(int x, int y)
            {
                return
                    y < 0 || y >= Chars.Length || x < 0 || x >= Chars[y].Length ? lineType.None :
                    "─└┌├┬┴╓╙╨╟╥╫┼".Contains(Chars[y][x]) ? lineType.Single :
                    "═╚╔╠╦╩╒╘╧╞╤╪╬".Contains(Chars[y][x]) ? lineType.Double : lineType.None;
            }
            public lineType BottomLine(int x, int y)
            {
                return
                    y < 0 || y >= Chars.Length || x < 0 || x >= Chars[y].Length ? lineType.None :
                    "│┌┐├┤┬╒╕╡╞╤╪┼".Contains(Chars[y][x]) ? lineType.Single :
                    "║╔╗╠╣╦╓╖╢╟╥╫╬".Contains(Chars[y][x]) ? lineType.Double : lineType.None;
            }
            public bool AnyLine(int x, int y)
            {
                return "─│┌┐└┘├┤┬┴┼═║╒╓╔╕╖╗╘╙╚╛╜╝╞╟╠╡╢╣╤╥╦╧╨╩╪╫╬".Contains(Chars[y][x]);
            }
            public int Width { get { return Chars[0].Length; } }
            public int Height { get { return Chars.Length; } }

            private string dir2str(direction d, lineType lin)
            {
                return
                    lin == lineType.Single ? (d == direction.Up ? "↑" : d == direction.Down ? "↓" : d == direction.Left ? "←" : "→") :
                    lin == lineType.Double ? (d == direction.Up ? "⇑" : d == direction.Down ? "⇓" : d == direction.Left ? "⇐" : "⇒") : "";
            }

            public string GetLineShape(int x, int y, direction dir, int minX, int minY, int maxX, int maxY)
            {
                var lnType = dir == direction.Up ? TopLine(x, y) : dir == direction.Right ? RightLine(x, y) : dir == direction.Down ? BottomLine(x, y) : LeftLine(x, y);
                string ret = dir2str(dir, lnType);
                while (true)
                {
                    switch (dir)
                    {
                        case direction.Up:
                            y--;
                            break;
                        case direction.Left:
                            x--;
                            break;
                        case direction.Down:
                            y++;
                            break;
                        case direction.Right:
                            x++;
                            break;
                    }
                    var arr = new[] { x > minX && x < maxX && y > minY ? TopLine(x, y) : lineType.None,
                                               x < maxX && y > minY && y < maxY ? RightLine(x, y) : lineType.None, 
                                               x > minX && x < maxX && y < maxY ? BottomLine(x, y) : lineType.None, 
                                               x > minX && y > minY && y < maxY ? LeftLine(x, y) : lineType.None };
                    var count = arr.Count(l => l != lineType.None);
                    if (count == 1)
                        return ret;
                    if (count != 2)
                        return null;
                    dir = dir != direction.Down && arr[0] != lineType.None ? direction.Up :
                            dir != direction.Left && arr[1] != lineType.None ? direction.Right :
                            dir != direction.Up && arr[2] != lineType.None ? direction.Down :
                            dir != direction.Right && arr[3] != lineType.None ? direction.Left :
                            FuncitonHelper.ThrowParseError<direction>("The parser encountered an internal error.", x, y, SourceText);
                    ret += dir2str(dir, arr[(int) dir]);
                }
            }

            public int GetIndex(int x, int y)
            {
                int index = 0;
                while (y > 0)
                {
                    var p = SourceText.IndexOf('\n', index);
                    if (p == -1)
                        return SourceText.Length;
                    index = p;
                    y--;
                }
                return index + x;
            }
        }

        // Represents a node, which could be a box (declaration, call, literal, comment), a T-junction, cross-junction, or a loose end.
        private sealed class node
        {
            public int X { get; private set; }
            public int Y { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public nodeType Type { get; private set; }
            public node(int x, int y, int width, int height, nodeType type) { X = x; Y = y; Width = width; Height = height; Type = type; }
            public override string ToString() { return "({0}, {1}; {2}, {3}) = {4}".Fmt(X, Y, Width, Height, Type); }
            private string _contentCache;
            public string GetContent(sourceAsChars source)
            {
                return _contentCache ?? (_contentCache = string.Join("\n", Enumerable.Range(Y + 1, Height - 1).Select(i => new string(source.Chars[i].Subarray(X + 1, Width - 1)).Trim())));
            }
            public bool Deduce(edge[] edges, bool[] known, Dictionary<string, unparsedFunctionDeclaration> unparsedDeclarationsByName, Dictionary<node, unparsedFunctionDeclaration> unparsedDeclarationsByNode, Action<edge> isCorrect, Action<edge> isFlipped, sourceAsChars source)
            {
                switch (Type)
                {
                    case nodeType.Declaration:
                        // Declarations have only outputs, and they are always in the correct orientation because they define it
                        foreach (var e in edges)
                        {
                            if (e != null && e.StartNode == this)
                                isCorrect(e);
                            if (e != null && e.EndNode == this)
                                isFlipped(e);
                        }
                        Edges = edges;
                        Connectors = edges.Select(e => e == null ? connectorType.None : connectorType.Output).ToArray();
                        return true;

                    case nodeType.Call:
                        unparsedFunctionDeclaration func;
                        if (!unparsedDeclarationsByNode.TryGetValue(this, out func) && !unparsedDeclarationsByName.TryGetValue(GetContent(source), out func))
                            FuncitonHelper.ThrowParseError("Call to undefined function: {0}".Fmt(GetContent(source)), X, Y, source.SourceText);
                        if (func.Connectors.Count(fc => fc != connectorType.None) != edges.Count(e => e != null))
                            FuncitonHelper.ThrowParseError("Incorrect number of connectors on call to function: {0}".Fmt(GetContent(source)), X, Y, source.SourceText);
                        return deduceGiven(edges, known, isCorrect, isFlipped, () =>
                        {
                            FuncitonHelper.ThrowParseError("Incorrect orientation of connectors on call to function: {0}".Fmt(GetContent(source)), X, Y, source.SourceText);
                        }, func.Connectors);

                    case nodeType.Literal:
                        // Literals have only outputs
                        foreach (var e in edges)
                        {
                            if (e != null && e.StartNode == this)
                                isCorrect(e);
                            if (e != null && e.EndNode == this)
                                isFlipped(e);
                        }
                        Edges = edges;
                        Connectors = edges.Select(e => e == null ? connectorType.None : connectorType.Output).ToArray();
                        return true;

                    case nodeType.TJunction:
                        if (edges.Count(e => e != null) != 3)
                            FuncitonHelper.ThrowParseError("Incorrect number of connectors to T junction (this error indicates a bug in the parser; please report it).", X, Y, source.SourceText);
                        connectorType[] conn1 = { connectorType.Input, connectorType.Output, connectorType.None, connectorType.Output };
                        connectorType[] conn2 = { connectorType.Output, connectorType.Input, connectorType.None, connectorType.Input };
                        return deduceGiven(edges, known, isCorrect, isFlipped, () =>
                        {
                            FuncitonHelper.ThrowParseError("Incorrect orientation of connectors on T junction (this error indicates a bug in the parser; please report it).", X, Y, source.SourceText);
                        }, conn1, conn2);

                    case nodeType.CrossJunction:
                        if (edges.Count(e => e != null) != 4)
                            FuncitonHelper.ThrowParseError("Incorrect number of connectors to cross junction (this error indicates a bug in the parser; please report it).", X, Y, source.SourceText);
                        connectorType[] conn = { connectorType.Input, connectorType.Output, connectorType.Output, connectorType.Input };
                        return deduceGiven(edges, known, isCorrect, isFlipped, () =>
                        {
                            FuncitonHelper.ThrowParseError("Incorrect orientation of connectors on cross junction (this error indicates a bug in the parser; please report it).", X, Y, source.SourceText);
                        }, conn);

                    case nodeType.End:
                        // Ends have just one input
                        if (edges.Count(e => e != null) != 1)
                            FuncitonHelper.ThrowParseError("Incorrect number of connectors to end node (this error indicates a bug in the parser; please report it).", X, Y, source.SourceText);
                        connectorType[] conn3 = { connectorType.Input, connectorType.None, connectorType.None, connectorType.None };
                        return deduceGiven(edges, known, isCorrect, isFlipped, () =>
                        {
                            FuncitonHelper.ThrowParseError("Incorrect orientation of connectors to end node (this error indicates a bug in the parser; please report it).", X, Y, source.SourceText);
                        }, conn3);
                }

                FuncitonHelper.ThrowParseError("The parser encountered an internal error: unrecognised node type: {0}".Fmt(Type), X, Y, source.SourceText);
                return false;
            }

            public edge[] Edges { get; private set; }
            public connectorType[] Connectors { get; private set; }

            private bool deduceGiven(edge[] edges, bool[] known, Action<edge> isCorrect, Action<edge> isFlipped, Action throwIfInvalid, params connectorType[][] connectors)
            {
                bool[] validKnowns = null;
                edge[] validEdges = null;
                connectorType[] validConn = null;

                foreach (var conn in connectors)
                {
                    for (int rot = 0; rot < 4; rot++)
                    {
                        var rotatedEdges = edges.Skip(rot).Concat(edges.Take(rot)).ToArray();
                        var rotatedKnowns = known.Skip(rot).Concat(known.Take(rot)).ToArray();
                        var valid = Enumerable.Range(0, 4).All(i =>
                                (rotatedEdges[i] == null && conn[i] == connectorType.None) ||
                                (!rotatedKnowns[i] && conn[i] != connectorType.None) ||
                                (rotatedKnowns[i] && rotatedEdges[i].StartNode == this && conn[i] == connectorType.Output) ||
                                (rotatedKnowns[i] && rotatedEdges[i].EndNode == this && conn[i] == connectorType.Input));
                        if (valid)
                        {
                            if (validEdges != null)
                                return false;
                            validEdges = rotatedEdges;
                            validKnowns = rotatedKnowns;
                            validConn = conn;
                        }
                    }
                }

                if (validEdges == null)
                {
                    throwIfInvalid();
                    return false;
                }

                for (int i = 0; i < 4; i++)
                    if (validConn[i] != connectorType.None)
                        ((validEdges[i].EndNode == this) ^ (validConn[i] == connectorType.Input) ? isFlipped : isCorrect)(validEdges[i]);
                Edges = validEdges;
                Connectors = validConn;
                return true;
            }
        }

        private static direction opposite(direction dir)
        {
            switch (dir)
            {
                case direction.Up:
                    return direction.Down;
                case direction.Right:
                    return direction.Left;
                case direction.Down:
                    return direction.Up;
                case direction.Left:
                    return direction.Right;
                default:
                    throw new InvalidOperationException();
            }
        }

        private sealed class unfinishedEdge
        {
            public node StartNode;
            public direction DirectionFromStartNode;
            public int StartX, StartY, EndX, EndY;
            public direction DirectionGoingTo;
            public override string ToString()
            {
                return "[{0}] ({1}) → [{2}, {3}] ({4})".Fmt(StartNode, DirectionFromStartNode, EndX, EndY, DirectionGoingTo);
            }
        }

        private sealed class edge
        {
            public node StartNode { get; private set; }
            public direction DirectionFromStartNode;
            public node EndNode { get; private set; }
            public direction DirectionFromEndNode;
            public int StartX { get; private set; }
            public int StartY { get; private set; }
            public int EndX { get; private set; }
            public int EndY { get; private set; }
            public edge(node start, direction directionFromStart, node end, direction directionFromEnd, int startX, int startY, int endX, int endY)
            {
                StartNode = start;
                DirectionFromStartNode = directionFromStart;
                EndNode = end;
                DirectionFromEndNode = directionFromEnd;
                StartX = startX;
                StartY = startY;
                EndX = endX;
                EndY = endY;
            }
            public override string ToString()
            {
                return "[{0}] {1} → [{2}] {3}".Fmt(StartNode, DirectionFromStartNode, EndNode, DirectionFromEndNode);
            }
            public void Flip()
            {
                var n = StartNode;
                var d = DirectionFromStartNode;
                StartNode = EndNode;
                DirectionFromStartNode = DirectionFromEndNode;
                EndNode = n;
                DirectionFromEndNode = d;
            }
        }

        private abstract class unparsedDeclaration
        {
            public List<node> Nodes { get; private set; }
            public List<edge> Edges { get; private set; }
            protected sourceAsChars _source;
            protected unparsedDeclaration(List<node> nodes, List<edge> edges, sourceAsChars source)
            {
                Nodes = nodes;
                Edges = edges;
                _source = source;
            }

            public virtual FuncitonFunction Parse(Dictionary<string, unparsedFunctionDeclaration> unparsedFunctionsByName,
                Dictionary<node, unparsedFunctionDeclaration> unparsedFunctionsByNode,
                Dictionary<unparsedDeclaration, FuncitonFunction> parsedFunctions,
                BigInteger? stdin)
            {
                var processedEdges = new HashSet<edge>();

                Action<edge> isCorrect = e => { processedEdges.Add(e); };
                Action<edge> isFlipped = e =>
                {
                    if (processedEdges.Contains(e))
                        FuncitonHelper.ThrowParseError("Program is ambiguous: cannot determine the direction of this edge.", e.StartX, e.StartY, _source.SourceText);
                    e.Flip();
                    processedEdges.Add(e);
                };

                // Deduce all the inputs and outputs on every node
                var q = new Queue<node>(Nodes);
                var enqueued = 0;
                while (q.Count > 0)
                {
                    var node = q.Dequeue();
                    var edges = new[] { direction.Up, direction.Right, direction.Down, direction.Left }
                        .Select(dir => Edges.SingleOrDefault(e => (e.StartNode == node && e.DirectionFromStartNode == dir) || (e.EndNode == node && e.DirectionFromEndNode == dir))).ToArray();
                    var known = edges.Select(e => e != null && processedEdges.Contains(e)).ToArray();
                    if (!node.Deduce(edges, known, unparsedFunctionsByName, unparsedFunctionsByNode, isCorrect, isFlipped, _source))
                    {
                        q.Enqueue(node);
                        enqueued++;
                        if (enqueued == q.Count)
                        {
                            var funcName = this is unparsedFunctionDeclaration ? "function “{0}”".Fmt(((unparsedFunctionDeclaration) this).DeclarationName) : "the main program";
                            FuncitonHelper.ThrowParseError("Program is ambiguous: cannot determine the direction of all the edges in {0}.".Fmt(funcName));
                        }
                    }
                    else
                        enqueued = 0;
                }
                Ut.Assert(Nodes.All(n => n.Edges != null && n.Connectors != null));

                var outputs = new FuncitonFunction.Node[4];
                var func = createFuncitonFunction(outputs);
                parsedFunctions[this] = func;
                var edgesAlready = new Dictionary<edge, FuncitonFunction.Node>();
                var callNodesAlready = new Dictionary<node, FuncitonFunction.CallNode>();
                foreach (var node in Nodes.Where(n => n.Type == nodeType.End))
                {
                    Ut.Assert(node.Edges[0] != null);
                    Ut.Assert(node.Edges[1] == null && node.Edges[2] == null && node.Edges[3] == null);
                    outputs[(int) node.Edges[0].DirectionFromEndNode] = walk(func, node.Edges[0], edgesAlready, callNodesAlready, unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin);
                }
                return func;
            }

            protected abstract FuncitonFunction createFuncitonFunction(FuncitonFunction.Node[] outputs);

            private FuncitonFunction.Node walk(FuncitonFunction function, edge edge, Dictionary<edge, FuncitonFunction.Node> edgesAlready,
                Dictionary<node, FuncitonFunction.CallNode> callNodesAlready, Dictionary<string, unparsedFunctionDeclaration> unparsedFunctionsByName,
                Dictionary<node, unparsedFunctionDeclaration> unparsedFunctionsByNode, Dictionary<unparsedDeclaration, FuncitonFunction> parsedFunctions,
                BigInteger? stdin)
            {
                FuncitonFunction.Node tryNode;
                if (edgesAlready.TryGetValue(edge, out tryNode))
                    return tryNode;

                var node = edge.StartNode;
                switch (node.Type)
                {
                    case nodeType.TJunction:
                        if (node.Connectors[0] == connectorType.Output)
                        {
                            // NAND
                            var newNode = new FuncitonFunction.NandNode(function, new Position(_source.GetIndex(edge.StartX, edge.StartY), 1));
                            edgesAlready[edge] = newNode;
                            Ut.Assert(node.Connectors[3] == connectorType.Input);
                            newNode.Left = walk(function, node.Edges[3], edgesAlready, callNodesAlready, unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin);
                            Ut.Assert(node.Connectors[1] == connectorType.Input);
                            newNode.Right = walk(function, node.Edges[1], edgesAlready, callNodesAlready, unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin);
                            return newNode;
                        }
                        else
                        {
                            // splitter
                            Ut.Assert(node.Connectors[0] == connectorType.Input);
                            Ut.Assert(node.Connectors[1] == connectorType.Output);
                            Ut.Assert(node.Connectors[3] == connectorType.Output);
                            var walked = walk(function, node.Edges[0], edgesAlready, callNodesAlready, unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin);
                            edgesAlready[edge] = walked;
                            return walked;
                        }

                    case nodeType.CrossJunction:
                        Ut.Assert(node.Connectors[0] == connectorType.Input);
                        Ut.Assert(node.Connectors[1] == connectorType.Output);
                        Ut.Assert(node.Connectors[2] == connectorType.Output);
                        Ut.Assert(node.Connectors[3] == connectorType.Input);
                        FuncitonFunction.CrossWireNode newCrossWireNode;
                        if (node.Edges[1] == edge)
                        {
                            // less-than
                            newCrossWireNode = new FuncitonFunction.LessThanNode(function, new Position(_source.GetIndex(edge.StartX, edge.StartY), 1));
                        }
                        else
                        {
                            // shift-left
                            Ut.Assert(node.Edges[2] == edge);
                            newCrossWireNode = new FuncitonFunction.ShiftLeftNode(function, new Position(_source.GetIndex(edge.StartX, edge.StartY), 1));
                        }
                        edgesAlready[edge] = newCrossWireNode;
                        newCrossWireNode.Left = walk(function, node.Edges[0], edgesAlready, callNodesAlready, unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin);
                        newCrossWireNode.Right = walk(function, node.Edges[3], edgesAlready, callNodesAlready, unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin);
                        return newCrossWireNode;

                    case nodeType.Declaration:
                        var newInputNode = new FuncitonFunction.InputNode(function, new Position(_source.GetIndex(edge.StartX, edge.StartY), 1), (int) edge.DirectionFromStartNode);
                        edgesAlready[edge] = newInputNode;
                        return newInputNode;

                    case nodeType.Call:
                        unparsedFunctionDeclaration decl;
                        if (!unparsedFunctionsByNode.TryGetValue(node, out decl) && !unparsedFunctionsByName.TryGetValue(node.GetContent(_source), out decl))
                            FuncitonHelper.ThrowParseError("Call to undefined function “{0}”.".Fmt(node.GetContent(_source)), node.X, node.Y, _source.SourceText);

                        FuncitonFunction func;
                        if (!parsedFunctions.TryGetValue(decl, out func))
                            func = decl.Parse(unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin);

                        var outputPosition = Enumerable.Range(0, 4).First(i => edge.StartNode.Edges[i] == edge && edge.StartNode.Connectors[i] == connectorType.Output);

                        // Try to optimise away no-op functions
                        int? inputPosition = func.GetInputForOutputIfNop(outputPosition);
                        if (inputPosition != null)
                        {
                            Ut.Assert(node.Connectors[inputPosition.Value] == connectorType.Input);
                            var followInput = walk(function, node.Edges[inputPosition.Value], edgesAlready, callNodesAlready, unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin);
                            edgesAlready[edge] = followInput;
                            return followInput;
                        }

                        var newOutputNode = new FuncitonFunction.CallOutputNode(function, new Position(_source.GetIndex(edge.StartX, edge.StartY), 1), outputPosition);
                        edgesAlready[edge] = newOutputNode;

                        FuncitonFunction.CallNode callNode;
                        if (!callNodesAlready.TryGetValue(node, out callNode))
                        {
                            callNode = new FuncitonFunction.CallNode { Function = func };
                            callNodesAlready[node] = callNode;
                            callNode.Inputs = Enumerable.Range(0, 4)
                                .Select(i => node.Connectors[i] == connectorType.Input ? walk(function, node.Edges[i], edgesAlready, callNodesAlready, unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin) : null)
                                .ToArray();
                        }

                        newOutputNode.CallNode = callNode;
                        return newOutputNode;

                    case nodeType.Literal:
                        var content = Regex.Replace(node.GetContent(_source), @"\s*\n\s*", "").Trim().Replace('−', '-');
                        FuncitonFunction.Node newLiteralNode;
                        if (content.Length == 0)
                            newLiteralNode = new FuncitonFunction.StdInNode(function, new Position(_source.GetIndex(edge.StartX, edge.StartY), 1), stdin);
                        else
                        {
                            BigInteger literal;
                            if (!BigInteger.TryParse(content, out literal))
                                FuncitonHelper.ThrowParseError("Literal does not represent a valid integer.", node.X, node.Y, _source.SourceText);
                            newLiteralNode = new FuncitonFunction.LiteralNode(function, new Position(_source.GetIndex(edge.StartX, edge.StartY), 1), literal);
                        }
                        edgesAlready[edge] = newLiteralNode;
                        return newLiteralNode;

                    case nodeType.End:
                    case nodeType.Comment:
                    default:
                        FuncitonHelper.ThrowParseError("The parser encountered an internal error.", node.X, node.Y, _source.SourceText);
                        return null;
                }
            }

            public virtual connectorType[] Connectors
            {
                get
                {
                    var connectors = new connectorType[4];
                    foreach (var edge in Edges.Where(e => e.StartNode.Type == nodeType.End || e.EndNode.Type == nodeType.End))
                    {
                        var isStart = edge.StartNode.Type == nodeType.End;
                        var dir = isStart ? edge.DirectionFromStartNode : edge.DirectionFromEndNode;
                        if (connectors[(int) dir] != connectorType.None)
                            FuncitonHelper.ThrowParseError("Duplicate connector: ‘{0}’ is already an ‘{1}’.".Fmt(dir, connectors[(int) dir]), isStart ? edge.StartX : edge.EndX, isStart ? edge.StartY : edge.EndY, _source.SourceText);
                        connectors[(int) dir] = connectorType.Output;
                    }
                    return connectors;
                }
            }
        }

        private sealed class unparsedFunctionDeclaration : unparsedDeclaration
        {
            public string DeclarationName { get; private set; }
            public bool DeclarationIsPrivate { get; private set; }
            public node DeclarationNode { get; private set; }

            public unparsedFunctionDeclaration(List<node> nodes, List<edge> edges, sourceAsChars source)
                : base(nodes, edges, source)
            {
                var decls = Nodes.Where(n => n.Type == nodeType.Declaration).ToList();
                if (decls.Count > 1)
                    FuncitonHelper.ThrowParseError("Cannot have more than one declaration box connected with each other.", decls[0].X, decls[0].Y, _source.SourceText);

                DeclarationNode = decls[0];
                if (DeclarationNode.Height != 2)
                    FuncitonHelper.ThrowParseError("Declaration box must have exactly one line of content.", DeclarationNode.X, DeclarationNode.Y, _source.SourceText);

                foreach (var callbox in nodes.Where(n => n.Type == nodeType.Call))
                    if (callbox.Height != 2)
                        FuncitonHelper.ThrowParseError("Call box must have exactly one line of content.", callbox.X, callbox.Y, _source.SourceText);

                DeclarationName = new string(source.Chars[DeclarationNode.Y + 1].Subarray(DeclarationNode.X + 1, DeclarationNode.Width - 1)).Trim();
                if (DeclarationName.Length < 1)
                    FuncitonHelper.ThrowParseError("Function name missing.", DeclarationNode.X, DeclarationNode.Y, _source.SourceText);
                DeclarationIsPrivate = false;

                // Find the private marker
                var privateMarkerPosition = 0;
                var left = source.RightLine(DeclarationNode.X, DeclarationNode.Y + 1);
                if (left == lineType.Double)
                    FuncitonHelper.ThrowParseError("Unrecognised marker.", DeclarationNode.X, DeclarationNode.Y + 1, _source.SourceText);
                else if (left == lineType.Single)
                {
                    var shape = source.GetLineShape(DeclarationNode.X, DeclarationNode.Y + 1, direction.Right, DeclarationNode.X, DeclarationNode.Y, DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + DeclarationNode.Height);
                    if (shape == "→↑" || shape == "→↓")
                    {
                        DeclarationIsPrivate = true;
                        DeclarationName = new string(source.Chars[DeclarationNode.Y + 1].Subarray(DeclarationNode.X + 2, DeclarationNode.Width - 2)).Trim();
                        privateMarkerPosition = shape == "→↑" ? 1 : 3;
                    }
                    else
                        FuncitonHelper.ThrowParseError("Unrecognised marker.", DeclarationNode.X, DeclarationNode.Y + 1, _source.SourceText);
                }

                var right = source.LeftLine(DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + 1);
                if (right == lineType.Double)
                    FuncitonHelper.ThrowParseError("Unrecognised marker.", DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + 1, _source.SourceText);
                else if (right == lineType.Single)
                {
                    var shape = source.GetLineShape(DeclarationNode.X, DeclarationNode.Y + 1, direction.Left, DeclarationNode.X, DeclarationNode.Y, DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + DeclarationNode.Height);
                    if (shape == "←↑" || shape == "←↓")
                    {
                        if (DeclarationIsPrivate)
                            FuncitonHelper.ThrowParseError("Duplicate private marker.", DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + 1, _source.SourceText);
                        DeclarationIsPrivate = true;
                        DeclarationName = new string(source.Chars[DeclarationNode.Y + 1].Subarray(DeclarationNode.X + 1, DeclarationNode.Width - 2)).Trim();
                        privateMarkerPosition = shape == "←↑" ? 2 : 4;
                    }
                    else
                        FuncitonHelper.ThrowParseError("Unrecognised marker.", DeclarationNode.X + DeclarationNode.Width, DeclarationNode.Y + 1, _source.SourceText);
                }

                for (int i = DeclarationNode.X + 1; i < DeclarationNode.X + DeclarationNode.Width; i++)
                {
                    if ((i != DeclarationNode.X + 1 || privateMarkerPosition != 1) && (i != DeclarationNode.X + DeclarationNode.Width - 1 || privateMarkerPosition != 2))
                        if (source.BottomLine(i, DeclarationNode.Y) != lineType.None)
                            FuncitonHelper.ThrowParseError("Unrecognised marker.", i, DeclarationNode.Y, _source.SourceText);
                    if ((i != DeclarationNode.X + 1 || privateMarkerPosition != 3) && (i != DeclarationNode.X + DeclarationNode.Width - 1 || privateMarkerPosition != 4))
                        if (source.TopLine(i, DeclarationNode.Y + DeclarationNode.Height) != lineType.None)
                            FuncitonHelper.ThrowParseError("Unrecognised marker.", i, DeclarationNode.Y + DeclarationNode.Height, _source.SourceText);
                }
            }

            public override FuncitonFunction Parse(Dictionary<string, unparsedFunctionDeclaration> unparsedFunctionsByName,
                Dictionary<node, unparsedFunctionDeclaration> unparsedFunctionsByNode,
                Dictionary<unparsedDeclaration, FuncitonFunction> parsedFunctions,
                BigInteger? stdin)
            {
                FuncitonFunction func;
                if (parsedFunctions.TryGetValue(this, out func))
                    return func;

                return base.Parse(unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin);
            }

            public override connectorType[] Connectors
            {
                get
                {
                    var connectors = base.Connectors;
                    foreach (var edge in Edges.Where(e => e.StartNode == DeclarationNode || e.EndNode == DeclarationNode))
                    {
                        var isStart = edge.StartNode == DeclarationNode;
                        var dir = isStart ? edge.DirectionFromStartNode : edge.DirectionFromEndNode;
                        if (connectors[(int) dir] != connectorType.None)
                            FuncitonHelper.ThrowParseError("Duplicate connector: ‘{0}’ is already an ‘{1}’.".Fmt(dir, connectors[(int) dir]), isStart ? edge.StartX : edge.EndX, isStart ? edge.StartY : edge.EndY, _source.SourceText);
                        connectors[(int) dir] = connectorType.Input;
                    }
                    return connectors;
                }
            }

            protected override FuncitonFunction createFuncitonFunction(FuncitonFunction.Node[] outputs)
            {
                return new FuncitonFunction(outputs, DeclarationName);
            }
        }

        private sealed class unparsedProgram : unparsedDeclaration
        {
            public unparsedProgram(List<node> nodes, List<edge> edges, sourceAsChars source)
                : base(nodes, edges, source)
            {
            }

            protected override FuncitonFunction createFuncitonFunction(FuncitonFunction.Node[] outputs)
            {
                return new FuncitonProgram(outputs);
            }

            public new FuncitonProgram Parse(Dictionary<string, unparsedFunctionDeclaration> unparsedFunctionsByName,
                Dictionary<node, unparsedFunctionDeclaration> unparsedFunctionsByNode, Dictionary<unparsedDeclaration, FuncitonFunction> parsedFunctions,
                BigInteger? stdin)
            {
                return (FuncitonProgram) base.Parse(unparsedFunctionsByName, unparsedFunctionsByNode, parsedFunctions, stdin);
            }
        }
    }
}
