import { Book } from '../../model/book';

export type BookCreateRequest =
  Pick<Book, 'title' | 'author'> &
  Partial<Omit<Book, 'title' | 'author' | 'id'>>;
