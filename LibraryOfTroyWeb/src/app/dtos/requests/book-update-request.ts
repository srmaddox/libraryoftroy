import {Book} from '../../model/book';

export type BookUpdateRequest = Partial<Omit<Book, 'id'>>;
