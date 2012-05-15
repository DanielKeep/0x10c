
try:
    from itertools import islice, izip_longest

    EMPTY_BYTESTR = ""

except ImportError:
    from itertools import islice, zip_longest
    izip_longest = zip_longest

    EMPTY_BYTESTR = bytes()

from struct import pack

FILE_BASE='error'
SMAP_NAME='kernerr'
PREFIX='ERR_'

fi = open(FILE_BASE+'.i.lst', 'rt')

errors = []

with open(FILE_BASE+'.i.lst', 'rt') as fi:
    with open(FILE_BASE+'.i', 'wt') as fo:
        cur_id = 0
        for ln,line in enumerate(fi):
            line = line.strip()
            if line.startswith('#') or line == "":
                continue

            parts = line.split(None, 1)
            if len(parts) != 2:
                print("%d: skipping, doesn't have two parts." % ln)
                continue

            if not parts[0].startswith(PREFIX):
                print("%d: excluding %s: doesn't start with %s" % (
                    ln, parts[0], PREFIX))

            fo.write('.equ %s %d\n' % (parts[0], cur_id))
            errors.append((parts[0], parts[1]))
            cur_id += 1

def words_for_psz(s):
    return (len(s) + 2) >> 1

def grouper(n, iterable, fillvalue=None):
    "grouper(3, 'ABCDEFG', 'x') --> ABC DEF Gxx"
    args = [iter(iterable)] * n
    return izip_longest(fillvalue=fillvalue, *args)

def to_psz_iter(s):
    for (a,b) in grouper(2, s+'\0', '\0'):
        ac,bc = ord(a),ord(b)
        yield pack('>H', ((ac & 0x7f) << 8) | (bc & 0x7f))

def to_psz(s):
    return EMPTY_BYTESTR.join(to_psz_iter(s))

with open('../root/%s.smap' % SMAP_NAME, 'wb') as fo:
    fo.write(pack('>H', len(errors)))
    cur_off = 1 + 2*len(errors)
    for (i, (name, msg)) in enumerate(errors):
        fo.write(pack('>H', cur_off))
        cur_off += words_for_psz(name)
        fo.write(pack('>H', cur_off))
        cur_off += words_for_psz(msg)

    for (name, msg) in errors:
        fo.write(to_psz(name))
        fo.write(to_psz(msg))

