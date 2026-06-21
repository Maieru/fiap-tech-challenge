import type { ReactNode } from "react";
import { EmptyState } from "@/components/common/EmptyState";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

interface EntityTableColumn<T> {
  key: string;
  title: ReactNode;
  className?: string;
  render: (row: T) => ReactNode;
}

interface EntityTableProps<T> {
  data: T[];
  columns: EntityTableColumn<T>[];
  rowKey: (row: T) => string;
  emptyMessage?: string;
}

export function EntityTable<T>({ data, columns, rowKey, emptyMessage = "Nenhum registro encontrado." }: EntityTableProps<T>) {
  return (
    <div className="overflow-hidden rounded-lg border bg-card">
      <Table>
        <TableHeader>
          <TableRow>
            {columns.map((column) => (
              <TableHead key={column.key} className={column.className}>
                {column.title}
              </TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.length === 0 ? (
            <TableRow>
              <TableCell colSpan={columns.length}>
                <EmptyState message={emptyMessage} />
              </TableCell>
            </TableRow>
          ) : (
            data.map((row) => (
              <TableRow key={rowKey(row)}>
                {columns.map((column) => (
                  <TableCell key={column.key} className={column.className}>
                    {column.render(row)}
                  </TableCell>
                ))}
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}
