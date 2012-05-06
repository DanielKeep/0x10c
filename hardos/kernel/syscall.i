
.equ krn_syscall            0x0010
.equ krn_sysvar             krn_syscall + 2
.equ __krn_init             krn_sysvar + 2
.equ __krn_drv_init_addr    __krn_init + 2
.equ __krn_entry            __krn_drv_init_addr + 2

.equ krn_panic              0
.equ krn_terminate          krn_panic + 1
.equ krn_break              krn_terminate + 1
.equ krn_set_exit_handler   krn_break + 1
.equ krn_set_break_handler  krn_set_exit_handler + 1
.equ krn_run                krn_set_break_handler + 1

.equ __krn_empty_0000       krn_run + 1

.equ krn_con_getch          __krn_empty_0000 + 1
.equ krn_con_ignore         krn_con_getch + 1
.equ krn_con_clear          krn_con_ignore + 1
.equ krn_con_putch          krn_con_clear + 1
.equ krn_con_getcur         krn_con_putch + 1
.equ krn_con_setcur         krn_con_getcur + 1
.equ krn_con_getfmt         krn_con_setcur + 1
.equ krn_con_setfmt         krn_con_getfmt + 1

.equ krn_fs_exists          krn_con_setfmt + 1
.equ krn_fs_get_attrs       krn_fs_exists + 1
.equ krn_fs_first_file      krn_fs_get_attrs + 1
.equ krn_fs_next_file       krn_fs_first_file + 1

.equ krn_file_open          krn_fs_next_file + 1
.equ krn_file_close         krn_file_open + 1
.equ krn_file_seek          krn_file_close + 1
.equ krn_file_write         krn_file_seek + 1
.equ krn_file_read          krn_file_write + 1

.equ krn_drv_load_path      krn_file_read + 1
