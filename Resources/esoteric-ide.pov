#version 3.7;

background { color rgbt<0, 0, 0, 1> }

global_settings {
  assumed_gamma 1.0
  max_trace_level 5
  photons {
    spacing 0.02
    count 100000
  }
}

camera {
  right image_height/image_width
  location  <0,2.3,-4.5>
  look_at   <0,0,0>
}


light_source {
  <500,-350,-200>     // light's position
  color rgb 1         // light's color
  photons {           // photon block for a light source
    refraction on
    reflection on
  }
}     

light_source {
  0*x                 // light's position (translated below)
  color rgb 1.7       // light's color
  area_light
  <100, 0, 0> <0, 100, 0> // lights spread out across this distance (x * z)
  4, 4                // total number of lights in grid (4x*4z = 16 lights)
  adaptive 0          // 0,1,2,3...
  jitter              // adds random softening of light
  circular            // make the shape of the light circular
  orient              // orient light
  translate <500,0,500>   // <x y z> position of light
  photons {           // photon block for a light source
    refraction on
    reflection on
  }
}   

difference {          
  // Main sphere
  sphere { <0,0,0>, 1 }
                
  // Bubbles
  sphere { <-0.0238819657936142, -0.454423108815413, 0.23677424399032>, 0.0240166787635613 }
  sphere { <-0.269309265198796, -0.150573649979464, 0.434136230235052>, 0.0210371205494912 }
  sphere { <0.23949161415896, -0.293810844092542, -0.571133169145851>, 0.0352571050893781 }
  sphere { <0.416559323396794, -0.584664733887028, 0.47395336882861>, 0.0318840015269276 }
  sphere { <0.106966176585744, -0.186588547745062, 0.434480422844403>, 0.0373604790015893 }
  sphere { <0.1306608259355, 0.0715790163127608, 0.345722985149232>, 0.020918138865809 }
  sphere { <0.278088191187982, 0.257776726622962, -0.706763850388473>, 0.0299046295182335 }
  sphere { <0.397522899041661, 0.239539200365329, 0.325015328510206>, 0.0364328436071206 }
  sphere { <0.552077968396283, -0.268915544854903, 0.230728020533327>, 0.0259883330790272 }
  sphere { <0.36482465330736, -0.542040291029047, -0.148639006143733>, 0.0246572188589057 }

  material {
    texture {
      pigment { rgbt 1 }
      finish {
        ambient 0.0
        diffuse 0.01
        specular 0.6
        roughness 0.005
        reflection {
          0.1, 1
          fresnel on
        }
        conserve_energy
      }
    }
    interior {
      ior 1.7
      fade_power 1001
      fade_distance 0.9
      fade_color <0.5,0.6,0.8>
    }
  }
  photons {
    target 1.0
    refraction on
    reflection on
  }         
}         

// Greenish swirly thing          
isosurface {
  function {
    pow ( abs(x + 2*(pow(y,3) - y)), 2/3 ) + pow ( abs(y), 2/3 ) + pow ( abs(z), 2/3 ) - 1
  }
  max_gradient 13
  scale <.3, .8, .3>
  translate <0, .1, 0>
  rotate 45*y
  photons {
    target 1.0
    refraction on
    reflection on
  }
  pigment { color rgb <.2,1,.6> }
  finish { ambient .3 }
}

// Reddish swirly thing       
isosurface {
  function {
    pow ( abs(x + 2*(pow(y,3) - y)), 2/3 ) + pow ( abs(y), 2/3 ) + pow ( abs(z), 2/3 ) - 1
  }
  max_gradient 13
  scale <.1, .8, .1>
  rotate 80*z
  rotate 40*y
  photons {
    target 1.0
    refraction on
    reflection on
  }
  pigment { color rgb <1,.05,.2> }
  finish { ambient .3 }
}

// Saturn-like rings
#declare rotangle = -20*z + 15*x;
disc { <0, 0, 0> y, 1.32715229691339, 1.30000000000000 rotate rotangle pigment { color rgb <135,  97, 27>/255 } photons { target 1.0 refraction on reflection on } }
disc { <0, 0, 0> y, 1.41411054586717, 1.32715229691339 rotate rotangle pigment { color rgb <231, 166, 46>/255 } photons { target 1.0 refraction on reflection on } }
disc { <0, 0, 0> y, 1.51291165580643, 1.41411054586717 rotate rotangle pigment { color rgb <147, 105, 29>/255 } photons { target 1.0 refraction on reflection on } }
disc { <0, 0, 0> y, 1.62507607379233, 1.51291165580643 rotate rotangle pigment { color rgb <162, 117, 32>/255 } photons { target 1.0 refraction on reflection on } }
disc { <0, 0, 0> y, 1.68521722819899, 1.62507607379233 rotate rotangle pigment { color rgb <212, 152, 42>/255 } photons { target 1.0 refraction on reflection on } }
disc { <0, 0, 0> y, 1.84258624536106, 1.68521722819899 rotate rotangle pigment { color rgb <128,  92, 25>/255 } photons { target 1.0 refraction on reflection on } }
disc { <0, 0, 0> y, 1.86598580454755, 1.84258624536106 rotate rotangle pigment { color rgb <198, 142, 39>/255 } photons { target 1.0 refraction on reflection on } }
disc { <0, 0, 0> y, 1.93696935974852, 1.86598580454755 rotate rotangle pigment { color rgb <213, 153, 42>/255 } photons { target 1.0 refraction on reflection on } }
disc { <0, 0, 0> y, 1.96685566793515, 1.93696935974852 rotate rotangle pigment { color rgb <224, 161, 44>/255 } photons { target 1.0 refraction on reflection on } }
disc { <0, 0, 0> y, 2.03050870859554, 1.96685566793515 rotate rotangle pigment { color rgb <194, 140, 38>/255 } photons { target 1.0 refraction on reflection on } }
disc { <0, 0, 0> y, 2.20000000000000, 2.03050870859554 rotate rotangle pigment { color rgb <164, 118, 32>/255 } photons { target 1.0 refraction on reflection on } }

