import {Book} from '../../model/Book';

export type BookUpdateRequest = Partial<Omit<Book, 'id'>>;
