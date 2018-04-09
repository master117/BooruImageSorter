# AnimeImageSorter

This program sorts anime and similar images based on tags on booru boards and other websites into folders.
The code is C#, and should run (recompiled) on most devices, a python port was made by the great [BotterSnike, and can be found here](https://github.com/Bottersnike/AnimeImageSorter).

## Install & Run

- Move the files from the release folder into the folder to be sorted.
- Run the exe.
- Thats it.

## Options

### Directory

Enter a directory to be used (no slash at the end), or leave blank to use the current directory the exe is in.

### Sort by

Series: Series/Copyright to which the image belongs.

Character: Sort by depicted character.

### File Operations

Move: Move files.

Copy: Copy Files.

### MD5 Option

To save bandwidth this programm initially uses MD5 hashes for looking up images.
Sankaku and other booru images are by default named after their MD5 hash when downloading.

Hard: Calculate all hashes based on filecontent, lower success rate and speed but no false positives.

Soft: Use filenames as hashes, when they match the hash pattern, faster and better success rate, but may have false positives. 

This is faster as hashes don't need to be calculated, 
but may be wrong if a file has a name that is a valid hash, but doesn't belong to this file, which is extremely rare.

### Multiple Option
What happens if an image has multiple series/character tags.

Copies: Create a copy of the file per tag. Best sort, but requires the most space.

Mixed Folders: Creates folders with combined tags as name.

First: Uses the first tag.

Skip: Skips these files from sorting.

### Reverse Image Search
Enables Reverse Image Search, only if booru/hash search fails: 

- You need to have saucenaoapikey.txt and imgurapikey.txt in the same folder, filled with your own matching keys.

- Very slow, every used image is uploaded.

Yes: Enables Reverse Image Search on fail.

No: Disables Reverse Image Search on fail.


## Notes

This software is open source and may be distributed, modified, and used, but not sold commercially.

The author provides no guarantee/warranty on anything, including data loss or corruption.
