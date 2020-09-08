

# Shazam - Documentation

## Content

- Overview
- Documentation
- Visualisation
- Todo
- Vocabulary

## Overview

Application is made of four main parts:

1. *Shazam*

   Part called Shazam is responsible for loading and saving data to database. It also provides API for adding and recognizing songs. 

2. *Audio Processor*

   Audio processor adjusts audio data for Shazam to be able to understand them. Those modifications include downsampling and setting number of audio channels to one.

3. *Recognizer*

   This part handles arguably the most important part of the algorithm. Filtering audio data and deciding what is the best match.

4. *Visualizer*

   This part was made mainly for debugging purposes. It contains simple data visualizing  methods that help to see straight away modifications on audio data.

### Adding song to database

​	Audio file with sampling rate 48 kHz. This means that the audio data can contain real frequencies from 0 to 24 kHZ.  This data is monoized (see vocabulary) and then low pass filter is applied. By default only frequencies under 6 kHz are kept so by downsampling by factor of four we do not create any noise. Then hamming function and FFT is applied at every 1024 samples. 

> That leaves us with 512 bins (the other 512 bins is identical to the first 512) where each bin has a size of ~11.7 Hz (6000 / 512). With current sampling rate of 12 kHz giving us approx. 11 FFT cycles per second. 

​	Bins are separated into 6 sectors. In each sector the strongest bin with value above average of all 512 bins are selected and TFP is created. Those TFPs are then stored. 

> Average value of 512 can be switched with average value of all bins in song. However I got overall better results on testing audio with average of only 512 bins. That is because some songs have high scale of loudness. Thus some parts came up without any TFP.

![Audio processing route](/Documentation/Images/AudioProcessing.png "Audio processing route")

​	In the end TFPs are transformed into pairs of Addresses and SongValues. Each pair has an anchor TFP and actual TFP. For super fast lookup and comparisons Address is stored in unsigned integer and SongValue in unsigned long.

- Address contains frequency of an anchor, frequency of the actual point and delta time between those two points.

  >9 bits for anchor frequency (maximum of 2^9 = 512 different bits)
  >
  >9 bits for actual point frequency
  >
  >14 bits for delta time 

- Song value contains absolute anchor time and song ID. 

  >32 bits for anchor time 
  >
  >32 bits for song ID

  Those pairs are then stored into hash table where Key is Address and Value is List of SongValues.

### Recognizing song

​	Microphone records audio with sampling rate of 12 kHz using only one channel. Meaning first three steps of audio processing described above can be omitted. After obtaining all TFPs pairs similar to Address SongValue are created. Only this time SongValue is just absolute anchor time.

​	Again we get Address-SongValue hash table. Now for each address in recording we get all SongValues in the database. 

> We can do this in O(*n*) time where *n* is number of addresses , *n* is ~ 330 (10 sec * 11 samples * 3 major TFP on avg.)

​	Now we filter out SongValues that do not create TGZ (see vocabulary).  We are left with some songs that have common TGZs with the recording. 

> Songs with small number of TGZs can left out.

​	Last part is to determine how many notes are time coherent with the recording. That is done simply by computing deltas for each common address of recording and song (deltas of absolute anchor time in song and recording). 

> Note: abs. anchor times are obtained via address (either in database for song, or in the same data structure for recording)

​	Song with the most time coherent notes is picked.

![Song recognition](/Documentation/Images/SongRecognition.png "Song recognition path")

## Shazam

### Public methods

---

#### AddNewSong

```C#
void AddNewSong(string path, string name, string author)
```

​	**path**: path to the song relative to file */Resources/Songs*

​	**name**: name of the song which it will be saved in metadata.

​	**author**: author of the song which will be saved in metadata. 

**Description**

​	Analyses song and adds it into database so it can be recognized.

---

### RecognizeSong

```c#
string RecognizeSong()
```

​	**return**: 'songName by authorName'.

**Description**

​	Records audio for 10 seconds. Analyses the audio and finds best match in database.

---

#### ListSongs

```c#
void ListSongs(TextWriter output)
```

​	**output**: TextWriter instance to write to

**Description**

​	Lists all songs in database into **output**.

### Private

---

#### LoadFingerprints

```c#
void LoadFingerprints(string folderPath)
```

​	**folderPath**: folder with saved fingerprints as '.txt' (default value: Constants.FingerprintPath)

**Description**

​	Loads all fingerprints. Fingerprints must be saved as 'songID.txt'.

> This method uses several other helper methods

---

#### LoadMetada

```c#
void LoadMetada(string metadataPath)
```

​	**metadataPath**: path to metadata.csv file (default value: Constants.MetadataPath)

**Description**

​	Reads csv file containing metadata to files.

​	Structure of the file: songID;songName;artistName

---

#### SaveTFPs

```c#
void SaveTFPs(List<Tuple<uint,uint>> timeFrequencyPoints, in uint songID)
```

​	**timeFrequencyPoints**: List of TFPs

​	**songID**: ID of the song


**Description**

​	Creates a new fingerprint in the default folder.

---

#### SaveMetadata

```c#
void SaveMetadata(uint songID, string name, string author, string metadataPath)
```

​	**songID**: ID of the song

​	**name**: name of the song

​	**author**: name of the author

​	**metadataPath**: path to metadata.csv file (default value: Constants.MetadataPath)

**Description**

​	Appends new metadata record into the metadata.csv file.

---

#### RecordAudio

```c#
double[] RecordAudio(int length)
```

​	**length**: how long the recording should be

​	**return**: raw audio data

**Description**

​	Records audio at 12kHz sampling rate on one channel.

---

#### CreateTimeFrequencyPoints

```c#
List<Tuple<uint,uint>> CreateTimeFrequencyPoints(int bufferSize, double[] data, double sensitivity)
```

​	**bufferSize**: size of the window FFT will be applied to

​	**data**: raw audio data

​	**sensitivity**:  coefitient for picking strongest bins

**Description**

​	Applies FFT at every *bufferSize* data. Gets 512 bins. Those are separated into 6 parts based on index:

​		0-10, 11-20, 21-40, 41-80, 81-160, 161-512

From each bin strongest bins are selected. If the bins value is above average value of all 512 bins * *sensitivity* then Time-Frequency point is created.

> Average value of 512 bins can be switched to average value of all bins using method *GetBinAverage*. See second note at 'Adding Song To Database' section.



## Audio Processor

> Note: this class is static

### Private methods

---

#### StereoToMono

```c#
void StereoToMono(IAudioFormat audio)
```

​	**audio**: IAudioFormat instance of an audio

**Description**:

​	For each sample computes average of both channels and creates only one.

---

#### DownSample

```c#
double[] DownSample(double[] data, int downFactor, double sampleRate)
```

​	**data**: raw audio data

​	**downFactor**: factor of downsampling

​	**sampleRate**: original samplerate of *data*

**Description**

​	First Butterworth low pass filter is applied on data. 

> Butterworth low pass filter was taken from here [link](https://www.codeproject.com/Tips/1092012/A-Butterworth-Filter-in-Csharp)

​	Then for every *downFactor* samples average is computed and used as a single sample.



## FastFourierTransformation

> Note: this class is static

Several versions of FFT/DFT are implemented. However based on this article [link](https://www.codeproject.com/Articles/1095473/Comparison-of-FFT-Implementations-for-NET) and experimental testing. FFT customized to audio data was used.

---

#### FFT

```c#
void FFT(double[] data, bool normalize)
```

​	**data**: complex numbers alternating Real and Imaginary parts.

​	**normalize**: flag if data should be normalized after FFT (default value: false)

**Description**

​	Applies FFT on data. This is in place implementation using bit reverse. Data length must be power of two.

> Implementation inspired by Mr. Lomont [link](https://www.lomont.org/software/misc/fft/SimpleFFT.pdf)



## AudioReader

Class used for reading audio files and creating custom IAudioFormat instances.

---

#### GetSound

```c#
IAudioFormat GetSound(string path)
```

​	**path**: path to audio file 

​	**return**: IAudioFormat with data and metadata of the audio

**Description**

​	Currently supported formats: **wav**

​	File is read by bytes that are interpreted according to specific format.)



## Visualiser

There are two modes that can help with analyzing audio data in real time also referred to as Music modes. Those are derived from AbstactMode which implements IVisualiserMode interface. Third mode is spectogram.

> Interface secures common API
>
> Abstract mode implements common parts

Music modes play audio in the background and work in real time.

> Note: magic constants might apply in code. Their purpose is only to scale drawing so it fits the window. Those magic constants should be described in code.

​	Visualisation can be implemented in few lines of code:

~~~c#
var audio = AudioReader.GetSound("Songs/WithoutYou.wav");
if (audio.Channels == 2)
    AudioProcessor.StereoToMono(audio);
var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Frequencies);
window.Run();
~~~

___

#### Visualiser

```c#
Visualiser(short[] data, uint channelCount, uint sampleRate, VisualisationModes vm, int downSampleCoef)
```

​	**data**: audio data

​	**channelCount**: nubmer of channels 

​	**sampleRate**: sample rate of audio

​	**vm**: visualisation mode

​	**downSampleCoef**: coeficient of downsampling (default value: 1)

**Description**

​	Creates new SoundBuffer (**SMFL** library) and visualisation.

___

#### Run

```c#
void Run()
```

**Description**

​	Initiates the visualisation. Starts playing song and draws data on new window (**SFML** library).



### Amplitude mode

This mode draws raw audio data. 

- X axis = time
- Y axis = sample value 

![Amplitude mode](/Documentation/Images/AMPLITUDE.PNG)

### Frequency mode

This mode uses FFT and draws frequency spectrum.

- X axis = frequency (usually 0 - 24 kHz)
- Y axis = loudness

![Frequency mode](/Documentation/Images/FREQUENCIES.PNG)

​	FFT is applied at every *BufferSize* samples. Then values are normalized to fit window and drawn. Frequencies go from low to high. 

> Humans are used to interpret sounds logarithmically as it is in my other visualiser in C++ [github](https://github.com/JanKleprlik/AudioVisualiser).

### Spectogram

Spectogram processes whole song at the time and then displays graph.

- X axis = time
- Y axis = frequency (usually 0 - 24 kHz)
- Brightness = loudness

![Spectogram](Documentation/Images/SPECTOGRAM.PNG)

This is basically Frequency mode for the whole song.

##  Todo

There are some things that can be improved or added. This is a list of some of them.

- When song is not recognized return closest match info (currently only in 'debug' mode). 

- Add support for more audio formats
- Add support for more sampling frequencies

## Vocabulary

| Word       | Meaning                                                      |
| ---------- | ------------------------------------------------------------ |
| to monoize | Transform multiple audio channels into one channel           |
| FFT        | Fast Fourier Transformation                                  |
| TFP        | Time Frequency Point - used in Shazam algorithm              |
| TGZ        | Target Zone (Zone created out of 5 TFPs + anchor) - used in Shazam algorithm |

In documentation in code I used specific type of brackets to determine what data structure it is.

> Those brackets can combine i.e.:
>
>  [person,(<friend,age>)] meaning dictionary with key of person and value is list of pairs friend-age.

| Brackets   | Meaning                               |
| ---------- | ------------------------------------- |
| <*A*, *B*> | Tuple with items *A* and *B*          |
| [*K*, *V*] | Dictionary with key *K* and value *V* |
| (*A*)      | List of *A*                           |

 
