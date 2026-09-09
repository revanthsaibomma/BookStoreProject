/* =========================================================
   BOOKNEST - BOOK FILTER
   ========================================================= */

document.addEventListener("DOMContentLoaded", function () {

    const genreButtons =
        document.querySelectorAll(".genre-btn");

    const books =
        document.querySelectorAll(".book-item");

    const emptyMessage =
        document.getElementById("filterEmpty");


    if (genreButtons.length === 0 || books.length === 0) {
        return;
    }


    genreButtons.forEach(function (button) {

        button.addEventListener("click", function () {

            const selectedGenre =
                this.getAttribute("data-genre");


            /* Remove active from every button */

            genreButtons.forEach(function (btn) {

                btn.classList.remove("active");

            });


            /* Activate clicked button */

            this.classList.add("active");


            let visibleBooks = 0;


            /* Filter books */

            books.forEach(function (book) {

                const bookGenre =
                    book.getAttribute("data-genre");


                if (
                    selectedGenre === "all" ||
                    (
                        bookGenre &&
                        bookGenre.toLowerCase() ===
                        selectedGenre.toLowerCase()
                    )
                ) {

                    book.style.display = "";

                    visibleBooks++;

                }
                else {

                    book.style.display = "none";

                }

            });


            /* Show empty message if necessary */

            if (emptyMessage) {

                if (visibleBooks === 0) {

                    emptyMessage.style.display = "block";

                }
                else {

                    emptyMessage.style.display = "none";

                }

            }

        });

    });

});


document.addEventListener("DOMContentLoaded", function () {

    const searchInput = document.getElementById("bookSearch");
    const searchButton = document.getElementById("searchButton");
    const booksGrid = document.getElementById("booksGrid");
    const noBooksMessage = document.getElementById("noBooksMessage");

    if (!searchInput || !booksGrid) {
        return;
    }

    const bookCards = booksGrid.querySelectorAll(".book-card");

    function searchBooks() {

        const searchText = searchInput.value.trim().toLowerCase();

        let visibleBooks = 0;

        bookCards.forEach(function (card) {

            const bookName = (
                card.getAttribute("data-book-name") || ""
            ).toLowerCase();

            const author = (
                card.getAttribute("data-author") || ""
            ).toLowerCase();

            const genre = (
                card.getAttribute("data-genre") || ""
            ).toLowerCase();

            const matches =
                searchText === "" ||
                bookName.includes(searchText) ||
                author.includes(searchText) ||
                genre.includes(searchText);

            if (matches) {
                card.style.display = "";
                visibleBooks++;
            } else {
                card.style.display = "none";
            }
        });

        if (noBooksMessage) {
            noBooksMessage.style.display =
                visibleBooks === 0 ? "block" : "none";
        }
    }

    // Search while typing
    searchInput.addEventListener("input", searchBooks);

    // Search button
    if (searchButton) {
        searchButton.addEventListener("click", function () {
            searchBooks();
        });
    }

});