
.equ do_syscall             0x0010

.equ sys_panic              0
.equ scr_putpsz             1
.equ scr_putcr              2
.equ scr_advance            3
.equ scr_nl                 4
.equ scr_set_format         5
.equ scr_font_set_glyph     6
.equ ring_sizeof            7
.equ ring_init              8
.equ ring_get_capacity      9
.equ ring_get_length        10
.equ ring_get_free          11
.equ ring_add               12
.equ ring_add_addr          13
.equ ring_peek              14
.equ ring_peek_addr         15
.equ ring_pop               16
.equ ring_pop_addr          17

.macro syscall(id)
    set push, a
    set a, id
    jsr do_syscall
.end
