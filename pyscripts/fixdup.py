import psycopg2
import csv
import argparse


def delete_item(cursor, item_id):
    print("Deleting ", item_id)
    q = 'DELETE FROM "MyLineItems" WHERE "Id"=%s'
    cursor.execute(q, (item_id,))
    rows_deleted = cursor.rowcount
    print("Rows deleted: ", rows_deleted)


def dl_dups_csv(conn, csv_file):
    """
    docstring
    """
    print("Downloading duplicates...")
    cursor = conn.cursor()

    query = 'SELECT COUNT(*), "LineItemId" FROM public."MyLineItems" GROUP BY public."MyLineItems"."LineItemId"  HAVING COUNT(*) > 1'
    cursor.execute(query)
    print("Duplicates count: ", cursor.rowcount)
    dups = {}
    for cnt, line_item_in in cursor.fetchall():
        dups[line_item_in] = cnt
    cursor.close()

    with open(csv_file, 'wt', newline='', errors='ignore', encoding='utf8') as outfile:
        has_columns = False
        writer = csv.writer(outfile)

        for d in dups:
            q = 'SELECT * FROM public."MyLineItems" WHERE "LineItemId"=%s'
            cursor = conn.cursor()
            cursor.execute(q, (d,))
            for idx, item in enumerate(cursor.fetchall()):
                if idx == 0 and not has_columns:
                    columns = [c[0] for c in cursor.description]
                    writer.writerow(columns)
                    has_columns = True
                writer.writerow(item)
            cursor.close()


def delete_dups(conn, csv_file):
    """
    docstring
    """
    cusor = conn.cursor()
    cusor.execute("SET search_path TO public;")

    with open(csv_file, 'rt', encoding='utf8', newline='') as infile:
        reader = csv.reader(infile)
        for idx, item in enumerate(reader):
            if idx == 0:
                continue

            id = item[0]
            delete_item(cusor, id)

    conn.commit()


DB_HOST = '170.64.158.123'
DB_PORT = 5432
DB_NAME = 'thelonelykids'
DB_USER = 'postgres'
DB_PASS = 'Espelimbergo_122289'

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--download")
    parser.add_argument("--delete")
    args = parser.parse_args()

    conn = psycopg2.connect(
        database=DB_NAME, user=DB_USER,
        password=DB_PASS, host=DB_HOST, port=DB_PORT)

    if args.download:
        dl_dups_csv(conn, args.download)

    if args.delete:
        delete_dups(conn, args.delete)

    conn.close()
