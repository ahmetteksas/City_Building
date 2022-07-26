The initial renders had a transparent shadow, and we needed full opaque shadows for the weather controller;
We made the shadow sprite sheets with sprite packer, then poured black on each frame and applied a 5% gaussian blur;
Normally, make sure your shadows are 100 % black and opaque; then drop the alpha from 255 to 100
To fix the grey color problem in TextuePacker-Gimp, copy the shadow sprite sheet into a new gimp document (Photoshop 
works fine) - probably some strange thing about the TexturePacker image format
