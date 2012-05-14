
=================
The HarDOS Kernel
=================

:Author: Daniel Keep <daniel.keep@gmail.com>
:Version: 0.1

The Syscall Interface
=====================

Programs and drivers both communicate with the kernel via the syscall interface.
There are three possible designs:

1.  Using ``int SYSCALL`` to invoke a syscall dispatch handler with the syscall
    identifier by a register or stack value.

2.  Using ``jsr SYSCALL`` to invoke the dispatch handler.

3.  Fixing the address of all syscalls.

We have decided to go with option #2 for a few reasons:

*   The interrupt would first have to be processed by an interrupt handler.
    Although the syscall message could be prioritised, the cost cannot be
    eliminated.

*   The interrupt approach would mean that interrupts cannot be disabled without
    also disabling all communication to the kernel.

*   It would also mean extra cycle expense to access the original value of the
    A register.

*   In the best case, ``jsr SYSCALL`` will take 3 cycles whilst
    ``int SYSCALL`` would take 9 cycles.

*   The fixed address approach would necessarily require sequential syscall
    numbers.  It would also double the space needed for storing the syscall
    table.

Thus, the full protocol is:

1.  Prepare the stack and registers as you would were you calling the function
    directly.

2.  Place the syscall number into register Z.  If Z is being used, it will need
    to be pushed to stack first.

3.  Execute ``jsr do_syscall``.

4.  If the syscall *use varargs*, the caller must clean the stack.  If the
    syscall *does not use varargs*, then the callee will clean the stack.

5.  The result of the syscall will be in register A.

6.  If Z was being used, it should be popped from the stack.

Conventions
-----------

Any syscall which can fail and otherwise has no meaningful result will return
the error code in the A register.

Any syscall which can fail *and* has meaningful output *and* that output has
a sensible "invalid" value (such as ``NULL``), the syscall's final argument
will be a pointer at which to store the error code.  If the pointer is zero,
the error code will be discarded.

Any syscall which can fail *and* has meaningful output *but* that output has
no sensible "invalid" value, the syscall will return the error code in the A
register and the syscall's result will be stored at a pointer passed as the
first argument to the syscall.

Disk Subsystem
==============

The disk subsystem is designed to handle multiple disk drives.  On
initialisation, it scans the connected devices for supported drives before
adding all of them to the ``disk_devices`` array.

Drives in this array are assigned drive letters according to their position.
The boot drive is *always* the first drive in this array and is thus always
drive ``A``.  Subsequent drives are referred to as ``B``, ``C``, and so on.

All disk commands are fundamentally non-blocking.  However, the kernel provides
two versions of each command: one blocking and one non-blocking with a
completion counter increment.

For example, the command to read a sector into memory has these versions:

*   ``disk_read_sectors(id, sector, count, buffer)`` - blocking.
*   ``disk_read_sectors_async(id, sector, count, buffer, *counter, *error)``
    - non-blocking with completion counter increment.

Note that "completion counter increment" means that, upon completion, the
kernel will do the following::

    add [counter], 1

This means that a single counter can theoretically be used for flagging
completion of multiple operations.

Also note that there is no version with a direct callback.  The reason for this
is that HarDOS does not yet support multiple threads of execution.  Without
this, callbacks would need to be executed from within the interrupt handler
which is a *very bad idea, indeed*.  Once threads are supported, callback
variants of the non-blocking syscalls will be added.

    Actually, these can be handled by fudging the return address from the
    interrupt handler so that we jump into the callback, which then returns
    to the normal code.  Problem there is that the handler needs to do some
    extra work to preserve registers.  Better would be to return into a
    trampoline that preserves registers, launches the callback (from the stack),
    restores the registers and then returns to the preempted code.

Internally, the blocking calls work by making a non-blocking call and then
spinning on the completion counter.

Syscalls
--------

``disk_get_num_drives() -> word count``
    Get the number of connected, recognised disk drives.

``disk_has_disk(word id) -> bool``
    Determines whether or not the specified drive has media inserted.

``disk_read_sectors(word id, word sector, word count, word *buffer) -> error``
    Reads one or more sectors into memory.
