import { Book } from '../../model/Book';

export type BookCreateRequest =
  Pick<Book, 'title' | 'author'> &
  Partial<Omit<Book, 'title' | 'author' | 'id'>>;
