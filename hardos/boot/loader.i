
.equ BOOT_HIGH      0x7200

.equ BOOT_DD_ORG    0x7020
.equ BOOT_DD_END    0x70ff
.equ BOOT_ORG       0x7100
.equ BOOT_CUTOFF    0x7180
.equ BOOT_END       0x72ff
.equ BOOT_FS_ORG    0x7300
.equ BOOT_FS_END    0x73ff

.equ BOOT_VARS      0x7400
.equ BOOT_FS_VARS   0x7500
.equ BOOT_DD_VARS   0x7580

.equ BOOT_BUFFER_FS_0 0x7600
.equ BOOT_BUFFER_FS_1 0x7800

.equ fs_init        BOOT_FS_ORG
.equ fs_find_file   fs_init + 2
.equ fs_open_handle fs_find_file + 2
.equ fs_read        fs_open_handle + 2
.equ fs_get_driver  fs_read + 2

.equ BOOT_FS_CODE   fs_get_driver + 2

.equ dd_init        BOOT_DD_ORG
.equ dd_read_sector dd_init + 2
.equ dd_get_driver  dd_read_sector + 2
.equ dd_get_sector_size  dd_get_driver + 2

.equ BOOT_DD_CODE   dd_get_sector_size + 2
