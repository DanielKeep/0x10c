
/*
 * Syscall interface.
 */

extern int krn_syscall_0(int id);
extern int krn_syscall_1(int id, int arg0);
extern int krn_syscall_2(int id, int arg0, int arg1);
extern int krn_syscall_3(int id, int arg0, int arg1, int arg2);

/*
 * General types.
 */

typedef int *krn_psz_t;
typedef int krn_err_t;
typedef int krn_bool_t;

/*
 * Process control.
 */

typedef void (*krn_callback_t)();

extern void krn_terminate(int exitCode);
extern void krn_break();

extern krn_callback_t krn_set_exit_handler(krn_callback_t fn);
extern krn_callback_t krn_set_break_handler(krn_callback_t fn);

extern void krn_exec(krn_psz_t path);

/*
 * Terminal.
 */

#define KRN_CON_NO_BREAK    (0x01)
#define KRN_CON_NO_ECHO     (0x02)

#define FG_H    (0x8000)
#define FG_R    (0x4000)
#define FG_G    (0x2000)
#define FG_B    (0x1000)
#define BG_H    (0x0800)
#define BG_R    (0x0400)
#define BG_G    (0x0200)
#define BG_B    (0x0100)
#define BLNK    (0x0080)

extern int      krn_con_getch(int flags);
extern void     krn_con_ignore();
extern void     krn_con_clear(int fill);
extern void     krn_con_putch(int ch, int flags);
extern int      krn_con_getcur();
extern void     krn_con_setcur(int xy);
extern int      krn_con_getfmt();
extern void     krn_con_setfmt(int fmt);

/*
 * File.
 */

typedef void *krn_file_t;

#define KRN_FILE_WRITE  (0x0001)

#define KRN_ANCHOR_BEG  (0)
#define KRN_ANCHOR_CUR  (1)
#define KRN_ANCHOR_END  (2)

extern krn_bool_t krn_fs_exists(krn_psz_t path);
extern krn_psz_t *krn_fs_first_file();
extern krn_psz_t *krn_fs_next_file();

extern krn_file_t krn_file_open(krn_psz_t path, int flags);
extern void krn_file_close(krn_file_t file);
extern int krn_file_seek(krn_file_t file, int offset, int anchor);
extern void krn_file_write(krn_file_t file, int *buffer, int length);
extern void krn_file_read(krn_file_t file, int *buffer, int length);

/*
 * Utility functions.
 */
