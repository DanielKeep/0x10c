
========================
Super Simple File System
========================

:Author: Daniel Keep <daniel.keep@gmail.com>
:Version: 1.0
:Licence: http://creativecommons.org/publicdomain/zero/1.0/

SSFS is designed to be an as-simple-as-possible filesystem for use with the
DCPU-16.  Its simplicity is based on a number of restrictions:

* no directoties,
* 8.4 filenames,
* no more than 512 files,
* no files of > 64kw,
* no file permissions or attributes,
* no timestamps,
* no redundancy,
* no error-checking, and
* no volume label.

On the bright side, SSFS should be roughly as simple a filesystem as you can
reasonably have, making it easy to implement.

Syntax conventions
==================

This document uses the following sytax conventions:

``lp"..."``
    Indicates a little-endian packed ASCII string.
    
Structures
==========

SSFS has just three structures.

Header
------

The header (which doubles as the boot sector) contains the basic metadata for
the filesystem.  It is laid out as follows:

=========== ==================================================================
Words       Use
=========== ==================================================================
0 - 1       Jump to boot loader, zero if disk is not bootable.
2 - 3       lp"SSFS".
4           Version number = 1.0.  Major in 8 msb, minor in 8 lsb.
5           First free sector; zero if no free sectors.
6           First directory sector; must be >= 1.
7 - 31      Reserved, zero-filled.
32 - 512    Boot loader (if present).
=========== ==================================================================

The header is always in the first sector.  It can be followed by zero or more
sectors containing the remainder of the bootloader which could not fit into
the first sector; these sectors are considered part of the header.

Directory Entry
---------------

The filesystem uses eight directory sectors laid out sequentially; each sector
has 64 directory entries, giving 512 entries total.  Each entry has the
following form:

=========== ==================================================================
Words       Use
=========== ==================================================================
0 - 3       Little-endian, zero-filled, packed file name.
4 - 5       Little-endian, zero-filled, packed file extension.
6           Sector at which file data starts; zero if file is empty.
7           File length in words.    
=========== ==================================================================

The location of the directory sectors is *technically* arbitrary, although it
will typically come immediately after the header (and any additional boot loader
sectors).

A directory is empty if the first word of the entry is zero or 0xffff.  A zero
entry indicates that the entry is empty and that there are no further entries
in the directory.  A 0xffff entry indicates that the entry is empty but that
there might be further entries in the directory.

The 0xffff form should be avoided if practical.

Linked Sector
-------------

Each non-empty file is made up of one or more sectors linked together into a
list. All unused sectors are also linked together in the same manner. Each of
these sectors has the following form:

=========== ==================================================================
Words       Use
=========== ==================================================================
0           Next sector in list; zero if this is the last sector.
1 - 511     Sector data.
=========== ==================================================================

Operations
==========

Formatting
----------

Walking the directory
---------------------

Adding a file
-------------

Removing a file
---------------

Extending a file
----------------

Truncating a file
-----------------
