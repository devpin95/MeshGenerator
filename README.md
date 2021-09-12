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
        - [Image Maps](#image-maps)
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

---

### Important Functions
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

---

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
The simple noise algorithm is an implementation of the 2D noise function described by [Scratchapixel](https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/procedural-patterns-noise-part-1/creating-simple-2D-noise). 

|<img src="http://dpiner.com/projects/MeshGenerator/images/SimpleNoise.png" width="255">|
| ------------------------------------------------------------ |
| *Height map generated using a simple noise function.*        |

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

Here are a list of [smoothing function](https://github.com/devpin95/MeshGenerator/blob/231d62b59cb02b196a669abe96e7f5d1732ed750/Mesh%20Generator/Assets/Scripts/Classes/Smoothing.cs) implemented in this demo and a graph comparing each output in Figure 1:

| Name | Function |
| --- | --- |
|Linear|x|
|[Cosine](https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/procedural-patterns-noise-part-1/creating-simple-1D-noise)|(1 - cos(πx)) * 0.5|
|[Smoothstep](https://en.wikipedia.org/wiki/Smoothstep)|3x<sup>2</sup> - 2x<sup>3</sup>|
|[Perlin Smoothstep](https://www.scratchapixel.com/lessons/procedural-generation-virtual-worlds/perlin-noise-part-2/improved-perlin-noise)|6x<sup>5</sup> -15x<sup>4</sup> + 10x<sup>3</sup>|

<br/>

|<img src="http://dpiner.com/projects/MeshGenerator/images/SmoothingFunctions.png" width="500">|
|---|
|*<b>Figure 1</b> - Output of the cosine, smoothstep, Perlin smoothstep, and linear functions on [0,1]*|

<br/>

Applying simple noise to a mesh provides mediocre results, though not surprisingly. Only after applying blur can we get a decent looking terrain.

|<img src="http://dpiner.com/projects/MeshGenerator/images/SimpleNoiseMesh.png" width="500">| <img src="http://dpiner.com/projects/MeshGenerator/images/SimpleNoiseMeshBlur.png" width="500"> |
| ------------------------------------------------------------ | ----- |
| *A simple noise height map applied to a mesh* | *Same height map after default Gaussian blur operation* |

<br/>

#### Perlin Noise

|<img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoise.png" width="255">|
| ------------------------------------------------------------ |
| *Height map generated using a perlin noise function.*        |

| <img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoiseDomainWarp1.png" width="255"> | <img src="http://dpiner.com/projects/MeshGenerator/images/PerlinNoiseDomainWarp2.png" width="255"> |
| ------------------------------------------------------------ | ---------------- |
| *Perlin noise.*        | *Perlin noise with domain warp* |
| *Both functions sampled in [0, 2]* |

## Design
