
:BOOT_HIGH      .equ 0x7200

:BOOT_DD_ORG    .equ 0x7020
:BOOT_DD_END    .equ 0x70ff
:BOOT_ORG       .equ 0x7100
:BOOT_CUTOFF    .equ 0x7180
:BOOT_END       .equ 0x72ff
:BOOT_FS_ORG    .equ 0x7300
:BOOT_FS_END    .equ 0x73ff

:BOOT_VARS      .equ 0x7400
:BOOT_FS_VARS   .equ 0x7500
:BOOT_DD_VARS   .equ 0x7580

:BOOT_BUFFER_FS_0   .equ 0x7600
:BOOT_BUFFER_FS_1   .equ 0x7800

:fs_init            .equ BOOT_FS_ORG
:fs_find_file       .equ fs_init + 2
:fs_open_handle     .equ fs_find_file + 2
:fs_read            .equ fs_open_handle + 2
:fs_get_driver      .equ fs_read + 2

:BOOT_FS_CODE       .equ fs_get_driver + 2

:dd_init            .equ BOOT_DD_ORG
:dd_read_sector     .equ dd_init + 2
:dd_get_driver      .equ dd_read_sector + 2

:BOOT_DD_CODE       .equ dd_get_driver + 2
