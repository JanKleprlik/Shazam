# Specification - Shazam

### Content

- Aim
- Brief plan
- UI
- Limitations

## Aim

​	The goal of this program is to create an application for recognizing songs from short recorded samples. Similarly to well-known application [Shazam](https://www.shazam.com/). Application will be built according to original study ([link](https://www.ee.columbia.edu/~dpwe/papers/Wang03-shazam.pdf?fbclid=IwAR2hsjAncsqC3_nHGOBbKnW0LbqX3N746fpFQDf6YZWBEUxCuLyQ2uDf_bg)) and this detailed description ([link](http://coding-geek.com/how-shazam-works/?fbclid=IwAR3uaZ6CEX4SERxA8si0ACyoX41TmX8VgHpkvJd96TpYXFHheaU2VhovzQk)) by Christophe.

The application will allow user to add new songs to the database. Try to recognize a song from a recording. 

User interface will be done via command line with few simple tasks.

A **SFML** library for playing, recording and visualizing data will be used.

## Brief plan

	### Song recognition

​	Client will be able to record 10 seconds sample. This sample will be processed and its fingerprint will be made. Songs from database will be filtered out according to fingerprint resemblance. Songs  with strong fingerprint similarity will be processed further. Time coherency of notes of important notes in songs and recording will be measured and correct song will be returned. If there is not any strong fingerprint resemblance, error note will be returned.

### Adding songs

​	When adding a new song its audio file must be provided. This audio file will be analyzed. Audio data will be mono-ized, downsampled and analyzed on a base of audio spectrum. Based on audio spectrum fingerprint will be created and stored. This fingerprint is used for fast comparison with recordings.

## UI

​	User interface will be provided in a simple form of command line arguments. 

Basic arguments should be:

- Addition of a new song to the database
- Recognizing song from a short sample
- Listing songs in database

User guideline shall be provided when everything is implemented.

## Limitations

- Only *.wav* audio files will be supported. I chose *.wav* format because it is lossless, easy to use and I have worked with it before. Secondly library **SFML** does not support more common format *.mp3* files. Thirdly many converters between formats exists.
- Songs that can be processed will have to be sampled at 48000 Hz. There are basically two choices 48 kHz and 44,1 kHz. Those are two most common sampling rates. The reason I picked 48 kHz is that it is very easy to find any song at this sampling rate from YouTube (48 kHz is common sampling rate for videos) using [youtube-dl](https://youtube-dl.org/). Other sampling rates might be implemented later.
- Device that will take the recording must be able to record an mono audio at 12 kHz. This rate is because in the algorithm songs are downsampled. And recording at this sampling rate will eliminate any actual audio frequencies above 6 kHz. That help to get rid of any high pitch noise.