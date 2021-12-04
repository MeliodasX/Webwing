# Webwing
An old school single threaded server written in C# 

## Overview
Webwing is a simple old school single threaded server that runs on local host and returns supported data wrapped in HTML so it can be viewed on your browser.

## Usage
After the project has been built, it can be run through command line. 
It takes in three parameters:
> - root = [Directory]
> - IP = [LocalHost]
> - port = [Any Available Port]

Thus the command would be something like
> Webwing root=C:\files\ IP=127.0.0.3 port=5500

The server can be accessed from server at the given IP and Port i.e., 127.0.0.3:5500/[path to file]

The logs of the operation are created in the build directory where the requests made to the server can be seen.

## Compatibility
- Only supports get requests
- Only fetches pictures, text files and gifs (Support for more filetypes can be added by editing the Mime.dat)

## Credits
Inspiration taken from Imtiaz Alam's article on c-sharpcorner.
