
.equ do_syscall             0x0010

.equ sys_panic              0
.equ scr_putpsz             1
.equ scr_putcr              2
.equ scr_advance            3
.equ scr_nl                 4
.equ scr_set_format         5
.equ scr_font_set_glyph     6

.macro syscall(id)
    set push, a
    set a, id
    jsr do_syscall
.end
