# Shazam - Documentation

## Content

- Overview
- Shazam
- Audio processing
- Visualization
- Todo

## Overview

Application is made of three main parts:

1. Shazam

   Part called Shazam is responsible for loading and saving data to database. And more importantly filtering audio data, deciding what is the best match. 

2. Audio Processor

   Audio processor adjusts audio data for Shazam to be able to understand them. Those modifications include down sampling and setting number of audio channels to one.

3. Visualizer

   This part was made mainly for debugging purposes. It contains simple data visualizing  methods that help to see straight away modifications on audio data.

## Todo

There are some things that can be improved or added. This is a list of some of them.

- When song is not recognized return closest match info (currently only in 'debug' mode). 

- Support more audio formats