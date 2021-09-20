# MeshGenerator

For this project, I explored the algorithms and techniques for generating terrain meshes, from simple noise to erosion and image manipulation. Below, you can find a guide on how to navigate the interactable terrain generator UI, along with examples and explanations of the algorithms used to generate a mesh.

<img src="http://dpiner.com/projects/MeshGenerator/images/Demo1.png" width="1000">

<br/>

## Table of Contents
- [Installation](#installation)
- [Usage](#guide)
- [Features](#features)
    - [Generating a Mesh](#generation-a-mesh)
    - [Important Functions](#important-functions)
        - [Remapping](#remapping)
    - [Noise Functions](#noise-functions)
        - [Sampling](#sampling)
        - [Simple Noise](#simple-noise)
        - [Perlin Noise](#perlin-noise)
        - [Octave Noise](#octave-noise)
        - [Image Maps](#image-maps-extra)
    - Simulations
    - Operations
- [Design](#design)

## Installation

- TODO create build and make installer
- TODO create WebGL build
- Maybe a docker container?


## Features

This section is dedicated to showing the algorithms used to generate terrain meshes. All of these algorithms are well known, so I will not being going too far in depth, but I will show the nuances that were needed to implement them for this project.

### Generating a Mesh
Arguably one of the most important problems to solve with terrain generation is how to generate a mesh at all. Fortunately, Unity provides object components that make rendering meshes easy, leaving us with only the work of telling it where each vertex is and which vertices are connected to one another.

Every mesh object in Unity has a list of vertices and a list of polygons (in this case, triangles). Both lists are flattened 2D arrays. The vertex list is a list of [3D Vectors](https://docs.unity3d.com/ScriptReference/Vector3.html) where each element represents a vertices location in 3D space. The triangles list is a list of integers where every three elements are the index of vertices in the vertex list that make up a single triangle.

The order of triangle group determines the direction of the triangle normal, which then determines which side of the triangle is visible to the camera (this is to cull traingles that are facing away, or on the other side of an object, that don't need to be rendered). This table below has the order of vertices that will ensure the triangle is rendered when the camera is above the triangle. Because we limit the range of the camera in the demo, we can assume the camera will (almost) always be above the flat mesh and will be visible.

|<img src="http://dpiner.com/projects/MeshGenerator/images/Triangle.png" width="200"> <br/> <1, 2, 3>, <2, 3, 1>, <3, 1, 2> <br/> *Vertice order where triangle normal will face up (towards the camera)*|<img src="http://dpiner.com/projects/MeshGenerator/images/TriangleMesh.png" width="255"> <br/> <3, 1, 2, 3, 2, 4> <br/>*Triangle list that represents triangle A and B*|
|-----|----|

---

### Important Functions

---
#### Remapping
One of the most used functions in this demo is the Remap function. This function is very helpful when we have one range of values that we want to translate to another range of values. For example, given a value between 0 and 1, we can translate that value to another value in the range 0 to 100. Trivially, we can just multiply the value by 100. But what if we want the value to be in the range 0 to 105? We can use the value and it's relationship to it's range to get the new value.

The Remap function has the following signature:

    public static float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax)

Here, *from* is our original value, *fromMin* is the original minimum value, *fromMax* is the original maximum value, *toMin* is the desired minimum value, and *toMax* is the desired maximum value. Formally, 

    from ∈ [fromMin, fromMax]

and we want to map *from* to the value *y*, such that
    
    from → y ∈ [toMin, toMax]

Intuitively, we can show the implementation as using the proportion of our initial range to find the the value.

<img src="http://dpiner.com/projects/MeshGenerator/images/Remap.png" width="255">

##### Implementation

First, we get the distance from the current value to the minimum value in our current range (*fromAbs*), then the total length of our range (*fromLen*).

    var fromAbs  =  from - fromMin;
    var fromLen = fromMax - fromMin;

Then we can find our normal (*prop*), or the proportion of the total length our value is at.

    var prop = fromAbs / fromLen;

We can do the same for our target range, find the total length (*toLen*), then find the proportion of the new range we need to make up to get the new value (*toAbs*).

    var toLen = toMax - toMin;
    var toAbs = toLen * prop;

But, *toAbs* is just a distance; we need to add the minimum value of our target range to get our new value (*to*).

    var to = toAbs + toMin;

Now, we have remapped our value from the starting range to the target range.

<br/>

Here is the remap function in full:

    public static float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax) 
    {
        var fromAbs  =  from - fromMin;
        var fromLen = fromMax - fromMin;      
       
        var prop = fromAbs / fromLen;
 
        var toLen = toMax - toMin;
        var toAbs = toLen * prop;
 
        var to = toAbs + toMin;
       
        return to;
    }


<br/>


### Noise Functions

---

The goal of noise functions is to generate sequences that have no repeating sections (at least as far as a human can tell). This is important for terrain generation because any repeating patterns become exceedingly obvious, especially when viewing from a distance. Further, we can use noise functions and image manipulation to initially generate realistic terrains before we apply more expensive erosion algorithms. Below are the noise functions implemented in the interactable demo.

<br/>

#### Sampling

An important part of applying noise to our generated mesh is determining the range at which we want to sample our functions. Because we have a mesh of discrete vertices, we need to break up our sample range into equal parts. We can do this by simply remapping the coordinate of interest to the sample range. We know the current dimensions of our mesh, what coordinate we're looking at, and the range we want to sample our function at. We can call our [remap](#remapping) function like this:

    Remap(x, 0, dim, min, max)

where *x* is our mesh vertice index, *dim* is the width/height of the mesh, *min* is the lower bound of our sample range, and *max* is the upper bound of our sample range.

For example, if we are looking at the x coordinate 15 with a mesh dimension of 255, and we want to sample our noise function in [0, 5], we can call

    Remap(15, 0, 255, 0, 5)

to get our sample coordinate that we pass into the noise function. Further, because we have a 2D mesh, we do the same in the y-direction (in the implementation, only square sample ranges are considered).

<br/>

#### Simple Noise
---
The simple noise algorithm is an implementation of the 2D noise function described by [Scratchapixel](https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/procedural-patterns-noise-part-1/creating-simple-2D-noise). 

|<img src="http://dpiner.com/projects/MeshGenerator/images/SimpleNoise.png" width="255"> *Height map generated using a simple noise function.* |<img src="http://dpiner.com/projects/MeshGenerator/images/SimpleNoiseMesh.png" width="500"> *Map applied to mesh*|
| ------------------------------------------------------------ |--------- |

In essence, a 2D noise function generates a grid of random numbers called a lattice. The functions takes in an x and y coordinate to the grid. Any value outside the grid wraps back around. For example, for a 5x5 lattice, given the coordinate (5, 1) will return the value at position (0, 1), noting that (0, 0) is the origin of the grid. Intuitively, for a grid of any size, we are duplicate the grid to the left, right, up, and down to create a plane of values that we can sample from at any point. Though our implementation will only be take samples from x >= 0 and y >= 0, the function also allows for negative coordinates.

Because we are working with a 2D sampling grid, we need to do some math when our sample point lands between vertices (say we want to sample the point *(0.5, 0.25)*). We can do this with [bilinear interpolation](https://en.wikipedia.org/wiki/Bilinear_interpolation).

Here is the code snippet implementing bilinear interpolation

    float Q11 = Lattice.Instance.SampleLattice(xmin, ymin); // bottom left point
    float Q12 = Lattice.Instance.SampleLattice(xmin, yrounded); // top left point
    float Q21 = Lattice.Instance.SampleLattice(xrounded, ymin); // bottom right point
    float Q22 = Lattice.Instance.SampleLattice(xrounded, yrounded); // top right point
    
    // linear interpolation in the x direction
    float R1 = (xmax - modX) * Q11 + (modX - xmin) * Q21; // x on the line between Q11 and Q21, y = ymin
    float R2 = (xmax - modX) * Q12 + (modX - xmin) * Q22; // x on the line between Q12 and Q22, y = ymax
    
    // linear interpolation in the y direction (between R1 and R2)
    float p = (ymax - modY) * R1 + (modY - ymin) * R2;

where *xrounded* and *yrounded* are the values were wrapped back around the lattice after going off the edge.

<br/>

##### Smoothing

An extra step we can take to improve our simple noise function is to apply smoothing to our results. Because our lattice is a random grid of values between 0 and 1, the resulting height map will look blocky, with straight lines between points. Adding a smoothing function to the value returned from the simple noise function allows us to map to a function that has a smoother transition between 0 and 1.

<img src="http://dpiner.com/projects/MeshGenerator/images/SmoothingFuncAnim.gif" width="1000">

Here are a list of [smoothing function](https://github.com/devpin95/MeshGenerator/blob/231d62b59cb02b196a669abe96e7f5d1732ed750/Mesh%20Generator/Assets/Scripts/Classes/Smoothing.cs) implemented in this demo and a graph comparing each output in the graph below:

| Name | Function |
| --- | --- |
|Linear|x|
|[Cosine](https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/procedural-patterns-noise-part-1/creating-simple-1D-noise)|(1 - cos(πx)) * 0.5|
|[Smoothstep](https://en.wikipedia.org/wiki/Smoothstep)|3x<sup>2</sup> - 2x<sup>3</sup>|
|[Perlin Smoothstep](https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/perlin-noise-part-2/improved-perlin-noise)|6x<sup>5</sup> -15x<sup>4</sup> + 10x<sup>3</sup>|

<br/>

<p align="center"><img src="http://dpiner.com/projects/MeshGenerator/images/SmoothingFunctions.png" width="500"></p>
<p align="center">Output of the cosine, smoothstep, Perlin smoothstep, and linear functions on [0,1]*</p>

[comment]: <> (<br/>)

[comment]: <> (Applying simple noise to a mesh provides mediocre results, though not surprisingly. Only after applying blur can we get a decent looking terrain.)

[comment]: <> (|<img src="http://dpiner.com/projects/MeshGenerator/images/SimpleNoiseMesh.png" width="500">| <img src="http://dpiner.com/projects/MeshGenerator/images/SimpleNoiseMeshBlur.png" width="500"> |)

[comment]: <> (| ------------------------------------------------------------ | ----- |)

[comment]: <> (| *A simple noise height map applied to a mesh* | *Same height map after default Gaussian blur operation* |)

<br/>

#### Perlin Noise

---

Perlin noise is a standard, widely-used noise function often used for terrain generation. In this demo, I used the Unity pre-packaged [Perlin function](https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html), but added some features to extend it's output.

|<img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoise.png" width="255"> <br/> *Height map generated using a perlin noise function*|<img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoiseMesh.png" width="500"> *Map applied to a mesh*|
| ---- | ---- |

<br/>

##### Domain Warp

<img src="http://dpiner.com/projects/MeshGenerator/images/DomainWarpMesh.png" width="1000">

Domain warping is an advanced noise function that uses noise and various coefficients as input back into a noise function. Domain warping produces more interesting output from a noise function and can be used to create certain types of land formations. You can read more from someone a lot smarted than me here: [https://www.iquilezles.org/www/articles/warp/warp.htm](https://www.iquilezles.org/www/articles/warp/warp.htm)

My first implementation (by [IQ](https://www.iquilezles.org/index.html)) of domain warp produced weird results, with may more white that I wanted but still created an interesting pattern. When applying it to a mesh, artifacts in the height map was too high and left shard edges on peaks. 

You can see the output below:

| <img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoiseDomainWarp1.png" width="255"> <br/> *Perlin noise [0, 2]*| <img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoiseDomainWarp2.png" width="255"> <br/>  *Perlin noise with domain warp [0, 2]* |
| ------------------------------------------------------------ | ---------------- |

After looking around, I found a different [implementation](http://jsfiddle.net/fro5y0jm/15/) that broke the algorithm apart in a way that made more sense and produced a much nicer, cleaner map (after shifting the sample range):


| <img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoiseDomainWarp3.png" width="255"> <br/> *Perlin Noise [0, 100]* | <img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoiseDomainWarp4.png" width="255"> <br/> *Perlin noise with domain warp [0, 100]* |
| ------------------------------------------------------------ | ---------------- |

My first implementation used a variable called the *hurst exponent*, which was used as an exponent to a variable value inside the loop of the fbm function. The hurst exponent in my implementation proved to be too sensitive and produced wildly different values on the interval [0, 1].

    ...

    Vector2 q = new Vector2( 
        fbm( pos + new Vector2(0.0f,0.0f), _data.perlin.hurst ), // x
        fbm( pos + new Vector2(5.2f,1.3f), _data.perlin.hurst )  // y
    );
    
    Vector2 r = new Vector2( 
        fbm( pos + 4.0f * q + new Vector2(1.7f, 9.2f), _data.perlin.hurst ), // x
        fbm( pos + 4.0f * q + new Vector2(8.3f, 2.8f), _data.perlin.hurst ) // y
    );
    
    sample = fbm( pos + 4.0f * r, _data.perlin.hurst );

    ...

<br/>

    public float fbm(Vector2 pos, float hurst)
    {
        float t = 0.0f;
        int numOctaves = 2;

        for (int i = 0; i < numOctaves; ++i)
        {
            float f = Mathf.Pow(2.0f, (float) i);
            float a = Mathf.Pow(f, -hurst);
            
            float perlin = Mathf.PerlinNoise(f * pos.x, f * pos.y);
            float sample = Mathf.Clamp01(perlin); // make sure that the value is actually between 0 and 1

            t += a * sample;
        }

        return t;
    }

Moving to the new implementation removed the hurst exponent and found the *q* and *r* values independently from each other, then used separately to find the final sample value.

    Vector2 pos = new Vector2( xSamplePoint, zSamplePoint );
            
    Vector2 q = new Vector2(
        FmbBeta( pos, 60, _data.perlin.octaves ),       // x point
        FmbBeta(                                        // y point
            new Vector2( pos.x + 5.2f, pos.y + 1.3f ),  // shifted sample point
            60,                                         // scale
            _data.perlin.octaves                        // num octave
        )
    );
    
    Vector2 warpedPos = new Vector2(
        pos.x + _data.perlin.domainFactorX * q[0],      // x point + x warp
        pos.y + _data.perlin.domainFactorY * q[1]),     // y point + y warp
    );

    float qp = FmbBeta( warpedPos, 60, _data.perlin.octaves );

<br/>

    public float FmbBeta(Vector2 pos, float scale = 1.0f, int octaves = 1, float lacunarity = 2f, float gain = 0.5f)
    {
        float total = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; ++i)
        {
            float sample = Mathf.PerlinNoise(pos.x / scale * frequency, pos.y / scale * frequency) * amplitude;

            total += sample;
            frequency *= lacunarity;
            amplitude *= gain;
        }

        return total;
    }

Further work on this domain warp implementation would require a closer look at the minimum and maximum output of the final FbmBeta function. Currently, we use the number of octaves and a normalization value, considering each iteration of the loop could add a maximum value of *1* to *total*. However, this results in a small cluster of values, usually below the mid height value. Using a normalizing value of *octaves/2* produced a larger range of values, but requires more testing to ensure the output never falls outside out height map range [0, 1].

<br/>

##### Ridged Perlin Noise

<img src="http://dpiner.com/projects/MeshGenerator/images/RidgedPerlinNoiseMesh.png" width="1000">

Ridged Perlin noise is a simple operation on the output of the vanilla Perlin noise or the domain warp function. The operation takes first stretched the map on the range [-1, 1], then takes the absolute value of any point under 0, and remaps back to [0, 1]. This adds the effect of start ridges where the original terrain crossed it's mid height value. The operations output and implementation can be seen below.

Output:

| <img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoise.png" width="255"> <br/> *Perlin noise [0, 5]*| <img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoiseRidged.png" width="255"> <br/>  *Ridged Perlin noise [0, 5]* | <img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoiseRidged2.png" width="255"> <br/>  *Inverse Ridged Perlin noise [0, 5]* |
| ------------------------------------------------------------ | ---------------- | ---------- |

Implementation:

    float InvertAbs(float val)
    {
        val = Remap(val, 0, 1, -1, 1);
        val = Mathf.Abs(val);
        val *= -1;
        val = Remap(val, -1, 0, 0, 1);
        return val;
    }

Note, without multiplying by *-1* and remapping from [-1, 0] to [0, 1], we would just end up with the height map's inverse.

<br/>

#### Octave Noise

<img src="http://dpiner.com/projects/MeshGenerator/images/OctaveNoiseMesh.png" width="1000">

Octave noise is a powerful extension to any noise function, where building up several sample points at different frequencies and amplitudes adds more depth and texture to a noise function. In this implementation, we use successive Perlin noise samples to create an octave noise function, though octave noise can use any noise function.

Put simply, octave noise add together several samples taken with different frequencies and amplitudes. Frequency shifts the x and y coordinate where we sample our noise function, and amplitude multiplies that output. Ultimately, we end up with a function similar to

    float sample = Noise(frequency * x, frequency * y) * amplitude

At each octave (think of octave kind of like layers), we change the frequency and amplitude of our sample. In this implementation, we found the best result by doubling the frequency and halving the amplitude at each octave, as shown by the code snippet:

    float frq = 1;
    float amp = 1;
    float sample = 0;

    for ( int octave = 0; octave < count; ++octave ) {
        sample += Perlin(x * frq, y * frq) * amp;
        frq *= 2;
        amp /= 2;
    }



*Note, in the demo, you can set any frequency and amplitude you would like for each of the 5 octaves available.*

The summation of each octave produces a much more interesting mesh than the vanilla Perlin function, and provides a better starting point for further operations. The gif below shows how each layer stacks to create the final mesh.

<img src="http://dpiner.com/projects/MeshGenerator/images/OctaveNoiseAnim.gif" width="1000">

<br/>

#### Image Maps (EXTRA)

Image maps can be used to apply a pre-made height map to the mesh. Any image and be used as an image map, including color pictures, where the greyscale value of each pixel is used instead of it's RGB value. Images are sampled in a similar way to noise functions, the dimension of the mesh is remapped to the dimension of the image and samples are taken at integer values. Applying an image that is smaller than the mesh will result in sample points being reused and will result in a blocky mesh. The Gaussian Blur operation can be used to smooth the resulting mesh for better results.

|<img src="http://dpiner.com/projects/MeshGenerator/images/map5.png" width="255"> <br/> *Pre-made height map*|<img src="http://dpiner.com/projects/MeshGenerator/images/ImageMapMesh.png" width="500"> <br/> *Height map applied to 5x5 mesh*|
|-----|-----|

---

### Simulations

Simulating natural processes on a generated mesh add an extra layer of believability to a terrain, creating subtle features that go a long way to imitate what our brains would expect to see.

<br/>

#### Hydraulic Erosion
An important natural process is the movement of water down hills and mountain slopes called hydraulic erosion. Hydraulic erosion is the movement of sediment picked up by water as it moves across the surface of a terrain and depositing it somewhere further down the slope. On a large scale, this will create creases along mountain slopes and flatten the ground in valleys.

<img src="http://dpiner.com/projects/MeshGenerator/images/HydraulicErosionMesh1.png" width="1000">

##### Algorithm

This hydraulic erosion algorithm is a particle based simulation developed by [Nicholas McDonald](https://nickmcd.me/about/).

<p align="center"><img align="center" src="http://dpiner.com/projects/MeshGenerator/images/HydraulicErosionDemo1.gif" width="255"></p>

<p align="center">Hydraulic erosion visualization with 20 particles</p>

The process begins by randomly selecting a position in the map for the water particle to land.

    drop.pos.x = Random.Range(0f, mapdim);
    drop.pos.y = Random.Range(0f, mapdim);

At the beginning of it's life, the has a sediment level of *0*, *0* speed in the *x* and *y* directions, and a starting volume set by the user in the parameters. 

    drop.speed.x = 0;
    drop.speed.y = 0;
    drop.sediment = 0;
    drop.volume = parameters.StartingVolume;

The simulation continues as long as the drop has a volume above the threshold set by the user

    while (drop.volume > parameters.MINVolume) {
        ...
    }

The first step in an iteration is to save the drops initial position before applying any movement to the drop so that we a drop doesn't dig itself into a hole.

    Vector2 initialPos = drop.pos;

At the initial position, we find the normal of the vertex at the integer values of the position.

    norm = map.SampleBetaNormalAtXY((int)initialPos.x, (int)initialPos.y);

Using the normal, we can determine which way to move the drop. Using a physics-based approach, we set the speed of the drop using F=ma, where a = F/m.

    forceVector = new Vector2(norm.x, norm.z);
    Vector2 F = parameters.DT * forceVector; // force
    float m = (drop.volume * parameters.Density); // mass
    drop.speed += F / m;
    drop.pos += parameters.DT * drop.speed;

Here, *parameters.DT* is the time step set by the user and acts as a scaling factor.

After moving the drop, we apply friction to slow the particle:

    drop.speed *= 1f - parameters.DT * parameters.Friction;

Now that the drop has moved across the terrain, it picked up and deposited some sediment to it's initial position. We determine how much by the distance it traveled:

    float travelDistance = drop.speed.magnitude * (prevHeight - curHeight);
    float maxsed = drop.volume * travelDistance;

    if (maxsed < 0) maxsed = 0f;
    float seddiff = maxsed - drop.sediment;

where *prevHeight* was the drop's height at it's initial position and *curHeight* is the drop's height at it's current position. Here, *maxsed* is the maximum amount of sediment that the drop could have picked up between it's initial position and it's current position. *seddiff* is the difference between how much the drop could have and how much it actually has.

We can set the drop's new sediment level using *seddiff*

    drop.sediment += parameters.DT * parameters.DepositeRate * seddiff;

Using how much sediment the drop picked up or left, we can change the height map to reflect the effects the drop is having on the terrain:

    float amount = parameters.DT * drop.volume * parameters.DepositeRate * seddiff;
    map.ChangeNode((int)initialPos.x, (int)initialPos.y, amount);

Finally, we need a way to ensure that the drop slowly fades away and the loop can end. We do this with evaporation, which slowly lowers the drops volume over time:

    drop.volume *= 1f - parameters.DT * parameters.EvaporationRate;

As the drop begins moving across the surface, the amount of sediment picked up or deposited by the drop is determined by the distance that it travelled

##### Simulation Stability

After some testing, I discovered that as the drop's volume approached it's minimum value, it's mass was getting smaller and smaller. This meant that the speed was growing larger and larger as the simulation continued. The table below shows a few examples of this instability:

|<img src="http://dpiner.com/projects/MeshGenerator/images/HEStability1.gif" width="255">|<img src="http://dpiner.com/projects/MeshGenerator/images/HEStability2.gif" width="255">|<img src="http://dpiner.com/projects/MeshGenerator/images/HEStability3.gif" width="255">|
|---|---|---|

Solution: 
- limit the particle's speed based on a set max speed and the drop's volume 
- set a maximum iteration for the simulation


    while (drop.volume > parameters.MINVolume && iteration < Constants.maxDropIterations) {
        ...

        if (drop.speed.magnitude > Constants.maxDropSpeed)
        {
            ++maxspeedcount;
            drop.speed = drop.speed.normalized * Constants.maxDropSpeed * drop.volume;
        }
        
        if (maxspeedcount > Constants.maxSpeedThreshold) break;

        ...

        ++iteration;
    }

Results:

|<img src="http://dpiner.com/projects/MeshGenerator/images/HydraulicErosionStability4.gif" width="255">|<img src="http://dpiner.com/projects/MeshGenerator/images/HydraulicErosionStability5.gif" width="255">|<img src="http://dpiner.com/projects/MeshGenerator/images/HydraulicErosionStability6.gif" width="255">|
|---|---|---|

---

## Design

