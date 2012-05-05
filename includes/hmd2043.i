
.equ    HMD2043_ID_L                    0x4cae
.equ    HMD2043_ID_H                    0x74fa
.equ    HMD2043_VER                     0x07c2
.equ    HMD2043_MAN_L                   0x4948
.equ    HMD2043_MAN_H                   0x2154

.equ    HMD2043_QUERY_MEDIA_PRESENT     0x0000
.equ    HMD2043_QUERY_MEDIA_PARAMETERS  0x0001
.equ    HMD2043_QUERY_DEVICE_FLAGS      0x0002
.equ    HMD2043_UPDATE_DEVICE_FLAGS     0x0003
.equ    HMD2043_QUERY_INTERRUPT_TYPE    0x0004
.equ    HMD2043_SET_INTERRUPT_MESSAGE   0x0005
.equ    HMD2043_READ_SECTORS            0x0010
.equ    HMD2043_WRITE_SECTORS           0x0011

.equ    HMD2043_ERROR_NONE              0x0000
.equ    HMD2043_ERROR_NO_MEDIA          0x0001
.equ    HMD2043_ERROR_INVALID_SECTOR    0x0002
.equ    HMD2043_ERROR_PENDING           0x0003