using System;
using RT.Util.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Numerics;
using RT.Util;

namespace EsotericIDE.Languages
{
    sealed class MorningtonCrescent : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Mornington Crescent"; } }
        public override string DefaultFileExtension { get { return "mcresc"; } }
        public override string GetInfo(string source, int cursorPosition) { return ""; }
        public override System.Windows.Forms.ToolStripMenuItem[] CreateMenus(Func<string> getSelectedText, Action<string> insertText) { return new System.Windows.Forms.ToolStripMenuItem[0]; }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            var sourceLines = new List<Tuple<string, int>>();
            var index = 0;
            Match m;
            while ((m = Regex.Match(source, @"\r?\n")).Success)
            {
                sourceLines.Add(new Tuple<string, int>(source.Substring(0, m.Index), index));
                index += m.Index + m.Length;
                source = source.Substring(m.Index + m.Length);
            }
            if (source.Length > 0)
                sourceLines.Add(new Tuple<string, int>(source, index));
            return new morningtonCrescentEnv(sourceLines.ToArray(), input);
        }

        private sealed class morningtonCrescentEnv : ExecutionEnvironment
        {
            private Tuple<string, int>[] _program;
            private Dictionary<string, List<string>> _dic = new Dictionary<string, List<string>>();
            private string _currentStation = "Mornington Crescent";
            private Dictionary<string, object> _values = new Dictionary<string, object>();
            private object _accumulator;
            private Stack<int> _jumpstack = new Stack<int>();

            protected override IEnumerable<Position> GetProgram()
            {
                int instructionIndex = 0;

                while (true)
                {
                    if (instructionIndex >= _program.Length)
                        throw new Exception("The program ends before reaching Mornington Crescent. That is not allowed.");

                    var instruction = _program[instructionIndex++];
                    yield return new Position(instruction.Item2, instruction.Item1.Length);

                    var m = Regex.Match(instruction.Item1, @"^Take (.*) Line to (.*)$");
                    if (!m.Success)
                        throw new Exception("Invalid instruction.");

                    var line = m.Groups[1].Value;
                    var station = m.Groups[2].Value;

                    if (!_dic.ContainsKey(_currentStation))
                        throw new Exception("“{0}” station does not exist.".Fmt(_currentStation));
                    else if (!_dic.ContainsKey(station))
                        throw new Exception("“{0}” station does not exist.".Fmt(station));
                    else if (!(_dic[_currentStation].Contains(line) && _dic[station].Contains(line)))
                        throw new Exception("{0} Line doesn’t service both {1} and {2}.".Fmt(line, _currentStation, station));

                    var stationValue = _values.Get(station, station);
                    Func<BigInteger, BigInteger, object> arithmetic = null;

                    switch (station)
                    {
                        case "Upminster": arithmetic = (a, b) => a + b; goto case "(arithmetic)";
                        case "Chalfont & Latimer": arithmetic = (a, b) => a * b; goto case "(arithmetic)";
                        case "Cannon Street": arithmetic = (a, b) => b == 0 ? "" : (object) (a / b); goto case "(arithmetic)";
                        case "Preston Road": arithmetic = (a, b) => b == 0 ? "" : (object) (a % b); goto case "(arithmetic)";
                        case "Manor House": arithmetic = (a, b) => ~(a | b); goto case "(arithmetic)";
                        case "Holland Park": arithmetic = (a, b) => a & b; goto case "(arithmetic)";
                        case "Turnham Green": arithmetic = (a, b) => b > 0 ? a >> (int) b : a; goto case "(arithmetic)";
                        case "Stepney Green": arithmetic = (a, b) => b > 0 ? a << (int) b : a; goto case "(arithmetic)";
                        case "Bounds Green": arithmetic = (a, b) => BigInteger.Max(a, b); goto case "(arithmetic)";

                        case "(arithmetic)":
                            if (!(_accumulator is BigInteger) || !(stationValue is BigInteger))
                                goto default;
                            _values[station] = _accumulator;
                            _accumulator = arithmetic((BigInteger) stationValue, (BigInteger) _accumulator);
                            break;

                        case "Russell Square":
                            if (!(stationValue is BigInteger))
                                goto default;
                            _values[station] = _accumulator;
                            var stationValueBigInt = (BigInteger) stationValue;
                            _accumulator = stationValueBigInt * stationValueBigInt;
                            break;

                        case "Notting Hill Gate":
                            if (!(stationValue is BigInteger))
                                goto default;
                            _values[station] = _accumulator;
                            _accumulator = ~(BigInteger) stationValue;
                            break;

                        case "Parsons Green":
                            var str = _accumulator as string;
                            if (str == null)
                                goto default;
                            var match = Regex.Match(str, @"-?\d+");
                            _accumulator = match.Success ? BigInteger.Parse(match.Value) : BigInteger.Zero;
                            _values[station] = match.Success ? str.Substring(match.Index + match.Length) : "";
                            break;

                        case "Seven Sisters":
                            _accumulator = (BigInteger) 7;
                            break;

                        case "Charing Cross":
                            _values[station] = _accumulator;
                            if (stationValue is string)
                                _accumulator = ((string) stationValue).Length > 0 ? (BigInteger) (int) (((string) stationValue)[0]) : BigInteger.Zero;
                            else
                                _accumulator = char.ConvertFromUtf32((int) (BigInteger) stationValue);
                            break;

                        case "Paddington":
                            if (!(_accumulator is string) || !(stationValue is string))
                                goto default;
                            _values[station] = _accumulator;
                            _accumulator = (string) stationValue + (string) _accumulator;
                            break;

                        case "Gunnersbury":
                            _values[station] = _accumulator;
                            if (_accumulator is string && stationValue is BigInteger)
                                _accumulator = ((string) _accumulator).Substring(0, (int) (BigInteger) stationValue);
                            else if (_accumulator is BigInteger && stationValue is string)
                                _accumulator = ((string) stationValue).Substring(0, (int) (BigInteger) _accumulator);
                            else
                                goto default;
                            break;

                        case "Mile End":
                            _values[station] = _accumulator;
                            if (_accumulator is string && stationValue is BigInteger)
                                _accumulator = ((string) _accumulator).Substring(((string) _accumulator).Length - (int) (BigInteger) stationValue);
                            else if (_accumulator is BigInteger && stationValue is string)
                                _accumulator = ((string) stationValue).Substring(((string) stationValue).Length - (int) (BigInteger) _accumulator);
                            else
                                goto default;
                            break;

                        case "Upney":
                            if (!(stationValue is string))
                                goto default;
                            _values[station] = _accumulator;
                            _accumulator = ((string) stationValue).ToUpperInvariant();
                            break;
                        case "Hounslow Central":
                            if (!(stationValue is string))
                                goto default;
                            _values[station] = _accumulator;
                            _accumulator = ((string) stationValue).ToLowerInvariant();
                            break;
                        case "Turnpike Lane":
                            if (!(stationValue is string))
                                goto default;
                            _values[station] = _accumulator;
                            _accumulator = ((string) stationValue).Reverse().JoinString();
                            break;
                        case "Bank":
                            _values["Hammersmith"] = _accumulator;
                            _values[station] = _accumulator;
                            _accumulator = stationValue;
                            break;
                        case "Hammersmith":
                            _accumulator = stationValue;
                            break;

                        case "Temple":
                            _jumpstack.Push(instructionIndex);
                            break;
                        case "Angel":
                            if (!_accumulator.Equals(BigInteger.Zero))
                            {
                                instructionIndex = _jumpstack.Peek();
                                station = "Temple";
                            }
                            break;
                        case "Marble Arch":
                            _jumpstack.Pop();
                            break;

                        case "Mornington Crescent":
                            _output.Append(_accumulator.ToString());
                            yield break;

                        default:
                            _values[station] = _accumulator;
                            _accumulator = stationValue;
                            break;
                    }

                    _currentStation = station;
                }
            }

            public morningtonCrescentEnv(Tuple<string, int>[] program, string input)
            {
                _program = program;
                _accumulator = input;

                var data = @"Acton Town [Piccadilly] [District]
Aldgate [Metropolitan] [Circle]
Aldgate East [Hammersmith & City] [District]
Alperton [Piccadilly]
Amersham [Metropolitan]
Angel [Northern]
Archway [Northern]
Arnos Grove [Piccadilly]
Arsenal [Piccadilly]
Baker Street [Hammersmith & City] [Circle] [Metropolitan] [Bakerloo] [Jubilee]
Balham [Northern]
Bank [Central] [Waterloo & City] [Northern] [District] [Circle]
Barbican [Hammersmith & City] [Circle] [Metropolitan]
Barking [Hammersmith & City] [District]
Barkingside [Central]
Barons Court [Piccadilly] [District]
Bayswater [Circle] [District]
Becontree [District]
Belsize Park [Northern]
Bermondsey [Jubilee]
Bethnal Green [Central]
Blackfriars [Circle] [District]
Blackhorse Road [Victoria]
Bond Street [Jubilee] [Central]
Borough [Northern]
Boston Manor [Piccadilly]
Bounds Green [Piccadilly]
Bow Road [Hammersmith & City] [District]
Brent Cross [Northern]
Brixton [Victoria]
Bromley-by-Bow [Hammersmith & City] [District]
Buckhurst Hill [Central]
Burnt Oak [Northern]
Caledonian Road [Piccadilly]
Camden Town [Northern]
Canada Water [Jubilee]
Canary Wharf [Jubilee]
Canning Town [Jubilee]
Cannon Street [Circle] [District]
Canons Park [Jubilee]
Chalfont & Latimer [Metropolitan]
Chalk Farm [Northern]
Chancery Lane [Central]
Charing Cross [Bakerloo] [Northern]
Chesham [Metropolitan]
Chigwell [Central]
Chiswick Park [District]
Chorleywood [Metropolitan]
Clapham Common [Northern]
Clapham North [Northern]
Clapham South [Northern]
Cockfosters [Piccadilly]
Colindale [Northern]
Colliers Wood [Northern]
Covent Garden [Piccadilly]
Croxley [Metropolitan]
Dagenham East [District]
Dagenham Heathway [District]
Debden [Central]
Dollis Hill [Jubilee]
Ealing Broadway [Central] [District]
Ealing Common [Piccadilly] [District]
Earl's Court [District] [Piccadilly]
East Acton [Central]
Eastcote [Metropolitan] [Piccadilly]
East Finchley [Northern]
East Ham [Hammersmith & City] [District]
East Putney [District]
Edgware [Northern]
Edgware Road [Hammersmith & City] [Circle] [District]
Edgware Road [Bakerloo]
Elephant & Castle [Bakerloo] [Northern]
Elm Park [District]
Embankment [Northern] [Bakerloo] [Circle] [District]
Epping [Central]
Euston [Northern] [Victoria]
Euston Square [Hammersmith & City] [Circle] [Metropolitan]
Fairlop [Central]
Farringdon [Hammersmith & City] [Circle] [Metropolitan]
Finchley Central [Northern]
Finchley Road [Metropolitan] [Jubilee]
Finsbury Park [Piccadilly] [Victoria]
Fulham Broadway [District]
Gants Hill [Central]
Gloucester Road [Piccadilly] [Circle] [District]
Golders Green [Northern]
Goldhawk Road [Hammersmith & City] [Circle]
Goodge Street [Northern]
Grange Hill [Central]
Great Portland Street [Hammersmith & City] [Circle] [Metropolitan]
Greenford [Central]
Green Park [Jubilee] [Victoria] [Piccadilly]
Gunnersbury [District]
Hainault [Central]
Hammersmith [Piccadilly] [District]
Hammersmith [Hammersmith & City] [Circle]
Hampstead [Northern]
Hanger Lane [Central]
Harlesden [Bakerloo]
Harrow & Wealdstone [Bakerloo]
Harrow-on-the-Hill [Metropolitan]
Hatton Cross [Piccadilly]
Heathrow Terminal 4 [Piccadilly]
Heathrow Terminal 5 [Piccadilly]
Heathrow Terminals 1, 2, 3 [Piccadilly]
Hendon Central [Northern]
High Barnet [Northern]
Highbury & Islington [Victoria]
Highgate [Northern]
High Street Kensington [Circle] [District]
Hillingdon [Metropolitan] [Piccadilly]
Holborn [Piccadilly] [Central]
Holland Park [Central]
Holloway Road [Piccadilly]
Hornchurch [District]
Hounslow Central [Piccadilly]
Hounslow East [Piccadilly]
Hounslow West [Piccadilly]
Hyde Park Corner [Piccadilly]
Ickenham [Metropolitan] [Piccadilly]
Kennington [Northern]
Kensal Green [Bakerloo]
Kensington [District]
Kentish Town [Northern]
Kenton [Bakerloo]
Kew Gardens [District]
Kilburn [Jubilee]
Kilburn Park [Bakerloo]
Kingsbury [Jubilee]
King's Cross St. Pancras [Victoria] [Piccadilly] [Northern] [Circle] [Hammersmith & City] [Metropolitan]
Knightsbridge [Piccadilly]
Ladbroke Grove [Hammersmith & City] [Circle]
Lambeth North [Bakerloo]
Lancaster Gate [Central]
Latimer Road [Hammersmith & City] [Circle]
Leicester Square [Northern] [Piccadilly]
Leyton [Central]
Leytonstone [Central]
Liverpool Street [Circle] [Hammersmith & City] [Metropolitan] [Central]
London Bridge [Northern] [Jubilee]
Loughton [Central]
Maida Vale [Bakerloo]
Manor House [Piccadilly]
Mansion House [Circle] [District]
Marble Arch [Central]
Marylebone [Bakerloo]
Mile End [Central] [Hammersmith & City] [District]
Mill Hill East [Northern]
Moorgate [Northern] [Hammersmith & City] [Circle] [Metropolitan]
Moor Park [Metropolitan]
Morden [Northern]
Mornington Crescent [Northern]
Neasden [Jubilee]
Newbury Park [Central]
North Acton [Central]
North Ealing [Piccadilly]
Northfields [Piccadilly]
North Greenwich [Jubilee]
North Harrow [Metropolitan]
Northolt [Central]
North Wembley [Bakerloo]
Northwick Park [Metropolitan]
Northwood [Metropolitan]
Northwood Hills [Metropolitan]
Notting Hill Gate [Circle] [District] [Central]
Oakwood [Piccadilly]
Old Street [Northern]
Osterley [Piccadilly]
Oval [Northern]
Oxford Circus [Bakerloo] [Victoria] [Central]
Paddington [Bakerloo] [Circle] [District]
Paddington [Circle] [Hammersmith & City]
Park Royal [Piccadilly]
Parsons Green [District]
Perivale [Central]
Piccadilly Circus [Bakerloo] [Piccadilly]
Pimlico [Victoria]
Pinner [Metropolitan]
Plaistow [Hammersmith & City] [District]
Preston Road [Metropolitan]
Putney Bridge [District]
Queensbury [Jubilee]
Queen's Park [Bakerloo]
Queensway [Central]
Ravenscourt Park [District]
Rayners Lane [Metropolitan] [Piccadilly]
Redbridge [Central]
Regent's Park [Bakerloo]
Richmond [District]
Rickmansworth [Metropolitan]
Roding Valley [Central]
Royal Oak [Circle] [Hammersmith & City]
Ruislip [Metropolitan] [Piccadilly]
Ruislip Gardens [Central]
Ruislip Manor [Metropolitan] [Piccadilly]
Russell Square [Piccadilly]
Seven Sisters [Victoria]
Shepherd's Bush [Central]
Shepherd's Bush Market [Circle] [Hammersmith & City]
Sloane Square [Circle] [District]
Snaresbrook [Central]
South Ealing [Piccadilly]
Southfields [District]
Southgate [Piccadilly]
South Harrow [Piccadilly]
South Kensington [Piccadilly] [Circle] [District]
South Kenton [Bakerloo]
South Ruislip [Central]
Southwark [Jubilee]
South Wimbledon [Northern]
South Woodford [Central]
Stamford Brook [District]
Stanmore [Jubilee]
Stepney Green [Hammersmith & City] [District]
St. James's Park [Circle] [District]
St. John's Wood [Jubilee]
Stockwell [Victoria] [Northern]
Stonebridge Park [Bakerloo]
St. Paul's [Central]
Stratford [Central]  [Jubilee]
Sudbury Hill [Piccadilly]
Sudbury Town [Piccadilly]
Swiss Cottage [Jubilee]
Temple [Circle] [District]
Theydon Bois [Central]
Tooting Bec [Northern]
Tooting Broadway [Northern]
Tottenham Court Road [Northern] [Central]
Tottenham Hale [Victoria]
Totteridge & Whetstone [Northern]
Tower Hill [Circle] [District]
Tufnell Park [Northern]
Turnham Green [District]
Turnpike Lane [Piccadilly]
Upminster [District]
Upminster Bridge [District]
Upney [District]
Upton Park [Hammersmith & City] [District]
Uxbridge [Metropolitan] [Piccadilly]
Vauxhall [Victoria]
Victoria [Victoria] [Circle] [District]
Walthamstow Central [Victoria]
Wanstead [Central]
Warren Street [Northern] [Victoria]
Warwick Avenue [Bakerloo]
Waterloo [Bakerloo] [Northern] [Waterloo & City] [Jubilee]
Watford [Metropolitan]
Wembley Central [Bakerloo]
Wembley Park [Metropolitan] [Jubilee]
West Acton [Central]
Westbourne Park [Circle] [Hammersmith & City]
West Brompton [District]
West Finchley [Northern]
West Ham [Jubilee] [Hammersmith & City] [District]
West Hampstead [Jubilee]
West Harrow [Metropolitan]
West Kensington [District]
Westminster [Circle] [District] [Jubilee]
West Ruislip [Central]
Whitechapel [Hammersmith & City] [District]
White City [Central]
Willesden Green [Jubilee]
Willesden Junction [Bakerloo]
Wimbledon [District]
Wimbledon Park [District]
Woodford [Central]
Wood Green [Piccadilly]
Wood Lane [Circle] [Hammersmith & City]
Woodside Park [Northern]";
                var lines = Regex.Split(data, @"\r?\n", RegexOptions.Singleline).Where(l => !string.IsNullOrWhiteSpace(l));
                var matches = lines.Select(l => Regex.Match(l, @"^([^\[\]]*?)\s*((\[[^\[\]]*\]\s*)*)$", RegexOptions.Singleline));
                foreach (var m in matches)
                    foreach (Match m2 in Regex.Matches(m.Groups[2].Value, @"\[([^\[\]]*)\]"))
                        _dic.AddSafe(m.Groups[1].Value, m2.Groups[1].Value);
            }

            public override string DescribeExecutionState()
            {
                return "Current station: {1}{0}Accumulator: {2}{0}Jumpstack: {3}{0}{0}{4}".Fmt(
                    Environment.NewLine,
                    _currentStation,
                    _accumulator is string ? "\"{0}\"".Fmt(((string) _accumulator).CLiteralEscape()) : _accumulator.ToString(),
                    _jumpstack.Count > 0 ? _jumpstack.JoinString(", ") : "<empty>",
                    _values.OrderBy(kvp => kvp.Key).Select(kvp => "{0} = {1}".Fmt(kvp.Key, kvp.Value is string ? "\"{0}\"".Fmt(((string) kvp.Value).CLiteralEscape()) : kvp.Value.ToString())).JoinString(Environment.NewLine)
                );
            }
        }
    }
}
