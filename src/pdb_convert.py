#!/usr/bin/env python3
"""Convert a game PlayerDatabase (8-byte header + SQLite) to/from a plain .sqlite file.

  python pdb_convert.py unpack PlayerDatabase  out.sqlite
  python pdb_convert.py pack   out.sqlite      PlayerDatabase_new --template PlayerDatabase
"""
import argparse
import sys

HEADER_LEN = 8
SQLITE_MAGIC = b"SQLite format 3\x00"


def unpack(src, dst):
    data = open(src, "rb").read()
    if data[HEADER_LEN:HEADER_LEN + 16] != SQLITE_MAGIC:
        sys.exit("error: SQLite signature not found at offset 8")
    open(dst, "wb").write(data[HEADER_LEN:])
    print(f"header {data[:HEADER_LEN].hex()} stripped; wrote {len(data) - HEADER_LEN} bytes to {dst}")


def pack(src, dst, template):
    tmpl = open(template, "rb").read()
    header = tmpl[:HEADER_LEN]
    body = open(src, "rb").read()
    if body[:16] != SQLITE_MAGIC:
        sys.exit("error: input is not a SQLite database")
    target = len(tmpl) - HEADER_LEN
    if len(body) > target:
        sys.exit(f"error: database grew to {len(body)} bytes, original capacity is {target}. "
                 "Run VACUUM or remove data.")
    body += b"\x00" * (target - len(body))  # keep original fixed file size
    open(dst, "wb").write(header + body)
    print(f"wrote {len(header) + len(body)} bytes to {dst} (header {header.hex()} from template)")


if __name__ == "__main__":
    p = argparse.ArgumentParser()
    sub = p.add_subparsers(dest="cmd", required=True)
    u = sub.add_parser("unpack"); u.add_argument("src"); u.add_argument("dst")
    k = sub.add_parser("pack"); k.add_argument("src"); k.add_argument("dst")
    k.add_argument("--template", required=True, help="original PlayerDatabase (supplies header and size)")
    a = p.parse_args()
    unpack(a.src, a.dst) if a.cmd == "unpack" else pack(a.src, a.dst, a.template)
