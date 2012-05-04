
:krn_syscall            .equ 0x0010
:__krn_init             .equ krn_syscall + 2
:__krn_drv_init_addr    .equ __krn_init + 2
:__krn_entry            .equ __krn_drv_init_addr + 2

:krn_terminate          .equ 0x0000
:krn_break              .equ krn_terminate + 1
:krn_set_exit_handler   .equ krn_break + 1
:krn_set_break_handler  .equ krn_set_exit_handler + 1
:krn_exec               .equ krn_set_break_handler + 1

:krn_con_getch          .equ krn_exec + 1
:krn_con_ignore         .equ krn_con_getch + 1
:krn_con_clear          .equ krn_con_ignore + 1
:krn_con_putch          .equ krn_con_clear + 1
:krn_con_getcur         .equ krn_con_putch + 1
:krn_con_setcur         .equ krn_con_getcur + 1
:krn_con_getfmt         .equ krn_con_setcur + 1
:krn_con_setfmt         .equ krn_con_getfmt + 1

:krn_fs_exists          .equ krn_con_setfmt + 1
:krn_fs_get_attrs       .equ krn_fs_exists + 1
:krn_fs_first_file      .equ krn_fs_get_attrs + 1
:krn_fs_next_file       .equ krn_fs_first_file + 1

:krn_file_open          .equ krn_fs_next_file + 1
:krn_file_close         .equ krn_file_open + 1
:krn_file_seek          .equ krn_file_close + 1
:krn_file_write         .equ krn_file_seek + 1
:krn_file_read          .equ krn_file_write + 1

:krn_drv_load_path      .equ krn_file_read + 1
