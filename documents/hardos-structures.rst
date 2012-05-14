
========================
HarDOS Kernel Structures
========================

:Author: Daniel Keep <daniel.keep@gmail.com>

This document defines the various internal and public structures used in the
HarDOS kernel.

Basic types and constants
=========================

::

    .equ NULL   0

    .equ FALSE  0
    .equ TRUE   1


``RingBuffer``
==============

=========== =================== ================================================
Offset      Type                Description
=========== =================== ================================================
``0x00``    ``word``            ``capacity``
``0x01``    ``word``            ``used``
``0x02``    ``word``            ``first``
``0x03``    ``word``            ``next``
``0x04``    ``word``            ``element_size``
``0x05``    ``word``            ``mask``
``0x06``    ``T...``            ``elements``
=========== =================== ================================================

``DiskCommand``
===============

This is the structure used for queueing disk commands.  It is effectively a
union of several command types.  The common prefix is:

=========== =================== ================================================
Offset      Type                Description
=========== =================== ================================================
``0x00``    ``word``            ``type``: a ``DISK_COMMAND_*`` constant.
``0x01``    ``word*``           ``complete``: pointer to be called upon
                                completion of the command.  ``NULL`` if no
                                callback needed.
``0x02``    ``word``            ``complete_arg_0``: a single-word argument to be
                                passed to the callback.
=========== =================== ================================================

For ``DISK_COMMAND_READ`` and ``DISK_COMMAND_WRITE`` commands:

=========== =================== ================================================
Offset      Type                Description
=========== =================== ================================================
``0x03``    ``word``            ``initial_sector``
``0x04``    ``word``            ``sector_count``
``0x05``    ``word``            ``buffer``
=========== =================== ================================================

::

    .equ DISK_COMMAND_READ      HMD2043_READ_SECTORS
    .equ DISK_COMMAND_WRITE     HMD2043_WRITE_SECTORS

``DiskDrive``
=============

This structure is used in the disk subsystem to keep track of disk drives known
to the system and their state/metadata.

=========== =================== ================================================
Offset      Type                Description
=========== =================== ================================================
``0x00``    ``word``            ``hardware_id``: id at which the device is
                                connected.
``0x01``    ``bool``            ``active``: ``TRUE`` if the device has supported
                                media inserted and is usable, ``FALSE``
                                otherwise.
``0x02``    ``word``            ``words_per_sector``: size, in words, of a disk
                                sector.
``0x03``    ``word``            ``sectors``: number of sectors on the disk.
``0x04``    ``word``            ``current_sector``: the sector which is
                                currently in the buffer. ``0xffff`` if the
                                buffer is unused.
``0x05``    ``word*``           ``buffer``: pointer to a buffer large enough to
                                contain one disk sector.
``0x06``    ``word``            ``pending``: is the drive currently performing a
                                non-blocking operation?
``0x07``    ``RingBuffer*``     ``command_queue``: pointer to a buffer of
                                ``DiskCommand``\ s.
=========== =================== ================================================

::

    .equ DISK_DRIVE_QUEUE_LENGTH    4
