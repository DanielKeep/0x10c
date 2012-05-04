
=====================================
Binary Image Exchange Format Proposal
=====================================

:Author: Daniel Keep <daniel.keep@gmail.com>
:Version: 1.0.3
:Licence: http://creativecommons.org/publicdomain/zero/1.0/
:Discussion: http://www.reddit.com/r/dcpu16/comments/t4xy2/

Recently, I've been working on some tools for creating disk images and
formatting said images with a filesystem.  At the same time, there have been
a number of people on Reddit asking about what byte order their tools should
be producing or consuming. It's also occurred to me that DCPU is a bit unusual
among emulated platforms in that some of the most prominent emulators right
from the get-go are actually web apps that users copy+paste programs into.

When I wrote up my floppy drive spec, I wanted to include a sample program with
a corresponding disk image.  Sadly, I couldn't attach a binary file, only text.
It occurrs to me that web-based emulators could very well have similar problems.

So, rather than get ahead of myself, I'd like to make a proposal and see how
the community responds.  Specifically, I'm looking for the opinions of tool
writers.

The idea
========

I'm proposing that we define a very simple encapsulation format to be used for
storing and exchanging binary files of various types.  Specifically, binary
dumps (or images) of data comprised of 16-bit words for the DCPU-16,
representing data such as machine code, ROM and disk images.

As an example, here is an image of a 1.44 MB disk formatted with zeroes and
the LE-packed string "Hello, World!" stored at the beginning::

 BIEF/0.1
 Type: Floppy
 Access: Read-Write
 Compression: Zlib
 Encoding: Base64
 Payload-Length: 2010
 
 eNrtwTENACAMALBJgR8hOMAAfEuW4P/ABGfbMzNHrXZrZ/QAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
 AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAPnq4MQRq

In addition to the basic encapsulation headers (Byte-Order, Compression,
Encoding and Content-Length) it also allows for special-purpose headers to be
added (Access and Type).

Benefits and drawbacks
======================

Things this format will give us:

  * A common format for binary image interchange that also specifies byte order.
  * A format with built-in compression, making internet transmission quicker.
  * A format which can be stored as plain text, suitable for submission to
    web apps and paste bins without risk of corruption.
  * A format which should be trivially readable in just about every programming
    language in the world [1]_.

Drawbacks:

  * Not as simple to read as an uncompressed, unencoded fixed byte order image.
  * Necessarily larger if files are stored without compression.

Open questions
==============

  * Is it worth having a little-endian mode at all?  It's still in the reference
    implementation, but I've removed it from the spec because I just can't come
    up with a logically sound reason for having it.
    
  * Is Content-Length worth having, or just throw it away.  It feels a bit like
    a security blanket: makes you feel better but doesn't really do anything.

Specifics
=========

No file extension is specified or required.

Each conforming file begins with the format identifier.  This is the five
characters "BIEF/" followed by a version identifier and terminated by a single
newline.  The version number described in this proposal is "0.1"; it will be
changed to "1.0" in the event the format is accepted by the community.

Newlines may be either a single linefeed (0x0a) byte *or* a carriage-return,
linefeed (0x0d, 0x0a) byte sequence.  Newlines should be written out as a
carriage-return, lifefeed sequence where possible [4]_.

This is followed by a sequence of zero or more headers.  Each
header is made up of a key, followed by a colon (':'), followed by the value
and finally terminated by a newline character.  Unless there is a
compelling reason to do so, keys and values should both the case insensitive.
Whitespace around the key and value should be ignored.  Headers should be
written in 7-bit ASCII, but read in as UTF-8 if possible [2]_.

Malformed or unrecognised headers should be ignored.  It is entirely possible
for the file to contain no headers at all.  In the case of duplicate headers,
the last value for a given header is used.

The headers are terminated by a zero-length line; that is, two consecutive
newlines.  If a file contains no headers, then it will start with a newline.

At this point, the rest of the file is the actual payload (or data) of the file.
The file should not contain any trailing bytes.

General headers
---------------

Byte-Order
    *Deprecated*.  This header is no longer a part of the base format. It will
    remain documented, however, until the format is accepted and finalised.
    
    "Big-Endian" or "Little-Endian", default if not specified is big-endian.
    
    Producing programs should endeavour to output files using big-endian by
    default.
    
Encoding
    "None" or "Base64", default if not specified is None.
    
    Producing programs should endeavour to output files using Base64 by default.
    
    The use of base64 allows for images to be passed around on the internet
    more easily: they can be put into gists or uploaded to pastebins.  This
    also avoids any potential problems with languages that represent binary data
    as Unicode or encoded 8-bit strings (like Javascript), and languages without
    8-bit safe strings (which I believe Lua is an example of).
    
Compression
    "None" or "Zlib", default if not specified is None.
    
    Producing programs should endeavour to output files using Zlib by
    default.
    
    Compression means a 1.44 MB disk image starting with "Hello, World!" is
    just over 2 KB instead of 1.44 MB.
    
    zlib was chosen thanks to its near total ubiquity.  Specifically, it means
    a deflate-compressed stream with the zlib header, *not* a raw deflate stream
    as Microsoft seems to love to use [3]_.
    
Payload-Length
    The value is a decimal integer representing the number of compressed,
    encoded bytes in the payload.
    
    Producing programs should include a Payload-Length header, consuming
    programs should *ideally* be prepared to operate without one.
    
    This header is included largely as a hedge against pastebins tacking on
    an errant newline here or there in the face of a particularly inflexible
    base64 implementation.

Disk image headers
------------------

This section is just normative; that is, I'm not attempting to standardise
this stuff, just giving a little extra context on what I'm personally using this
for.

Type
    Specifies the type of media the image represents.  Currently, I'm only using
    "Floppy", although I can see "Tape" and "Hard-Disk" being used in the
    future.
    
Access
    "Read-Write" or "Read-Only".  Used to implement the write-lock switch on
    the old 3.5" floppy.

Changelog
=========

1.0.3
    Changed proposal to be conforming reST.  Added identification line.  The
    major impetus for this was to allow the reference implementation to
    automatically fall back to raw mode if the line isn't present.
    
    Changed byte-order default to big-endian then proceeded to remove it from
    the spec entirely.  Most people seem to have settled on big-endian already,
    as much as I think that's a completely silly choice.  As Harold Lam would
    say: I'm a big enough man to admit when you're all bloody wrong. :P

1.0.2
	Added license.  Added language on default newline format for output.
	Clarified behaviour on reading duplicate headers.  Added changelog.
	Added link back to Reddit discussion thread.

	Clarified that this is for memory images, not bitmap images.

	Renamed "Content-Length" to "Payload-Length"; kudos to cheese_magnet.

1.0.1
	Fixed a mistake in the example disk image.

1.0.0
	Initial release.
    
License
=======

Licensed under the CC0 license:
http://creativecommons.org/publicdomain/zero/1.0/

To the extent possible under law, the author has waived all copyright and
related or neighboring rights to "Binary Image Exchange Format Proposal".
This work is published from: Australia.



.. [1]  Show me a language without good line-reading IO routines in its standard
        library, and I'll show you a rubbish language.  ;)

.. [2]  This is the "generous on input, strict on output" principle.

.. [3]  Incidentally, don't use System.IO.Compression.DeflateStream.  It's
        complete garbage.

.. [4]  This has been chosen because the most common text editor in the world
        is, probably (and unfortunately), Microsoft Notepad.  Notepad is also
        just about the *only* text editor left that refuses to recognise
        single linefeeds as newlines.  In this way, BIEF files opened in Notepad
        will not look "broken" to lay people.
