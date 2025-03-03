using LibraryOfTroyApi.DTOs;

using Srm.Soundex;

namespace LibraryOfTroyApi.Model;

public partial record Book {
    public static class Factory {
        public static Book FromCreateRequest ( BookCreateRequest request ) {
            if ( request is null ) {
                throw new ArgumentNullException ( nameof ( request ) );
            }

            // Calculate phonetic encodings
            string titleSoundex = string.Join(" ", request.Title.Split(new[] { ' ', ',', '-', '.', ';', ':' },
                                                StringSplitOptions.RemoveEmptyEntries)
                                    .Select(word => PhoneticMatcher.vowelSoundex(word)));

            string titleMetaphone = string.Join(" ", request.Title.Split(new[] { ' ', ',', '-', '.', ';', ':' },
                                                 StringSplitOptions.RemoveEmptyEntries)
                                     .Select(word => PhoneticMatcher.defaultMetaphone(word)));

            return new Book ( ) {
                Id = Guid.NewGuid ( ),
                Title = request.Title,
                Author = request.Author,

                Description = request.Description!,
                Publisher = request.Publisher!,
                PublicationDate = request.PublicationDate.Value!,
                Category = request.Category!,
                ISBN = request.ISBN!,
                PageCount = request.PageCount.Value!,

                CoverImageUrl = request.CoverImageUrl!,
                CoverImageAltText = request.CoverImageAltText!,

                TitleSoundex = titleSoundex,
                TitleMetaphone = titleMetaphone
            };
        }

        public static Book FromUpdateRequest ( BookUpdateRequest request, Book originalBook ) {
            if ( request is null ) {
                throw new ArgumentNullException ( nameof ( request ) );
            }

            if ( originalBook is null ) {
                throw new ArgumentNullException ( nameof ( originalBook ) );
            }

            string titleSoundex = originalBook.TitleSoundex;
            string titleMetaphone = originalBook.TitleMetaphone;

            if ( request.Title != null && request.Title != originalBook.Title ) {
                titleSoundex = string.Join ( " ", request.Title.Split ( new [] { ' ', ',', '-', '.', ';', ':' },
                                                     StringSplitOptions.RemoveEmptyEntries )
                                         .Select ( word => PhoneticMatcher.vowelSoundex ( word ) ) );

                titleMetaphone = string.Join ( " ", request.Title.Split ( new [] { ' ', ',', '-', '.', ';', ':' },
                                                      StringSplitOptions.RemoveEmptyEntries )
                                          .Select ( word => PhoneticMatcher.defaultMetaphone ( word ) ) );
            }

            return new Book ( ) {
                Id = originalBook.Id,
                Title = request.Title ?? originalBook.Title,
                Author = request.Author ?? originalBook.Author,
                Description = request.Description ?? originalBook.Description,
                Publisher = request.Publisher ?? originalBook.Publisher,
                PublicationDate = request.PublicationDate ?? originalBook.PublicationDate,
                Category = request.Category ?? originalBook.Category,
                ISBN = request.ISBN ?? originalBook.ISBN,
                PageCount = request.PageCount ?? originalBook.PageCount,
                CoverImageUrl = request.CoverImageUrl ?? originalBook.CoverImageUrl,
                CoverImageAltText = request.CoverImageAltText ?? originalBook.CoverImageAltText,
                TitleSoundex = titleSoundex,
                TitleMetaphone = titleMetaphone
            };
        }
    }
}
