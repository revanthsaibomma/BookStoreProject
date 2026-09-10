/* =========================================================
   BOOKNEST - BOOK SEARCH & GENRE FILTER
   ========================================================= */

document.addEventListener("DOMContentLoaded", function () {

    // Search elements
    const searchInput = document.getElementById("bookSearch");
    const searchButton = document.getElementById("searchButton");

    // Genre buttons
    const genreButtons = document.querySelectorAll(".genre-btn");

    // Book items
    const books = document.querySelectorAll(".book-item");

    // Empty messages
    const filterEmpty = document.getElementById("filterEmpty");
    const noBooksMessage = document.getElementById("noBooksMessage");

    // Currently selected genre
    let selectedGenre = "all";


    /* =====================================================
       STOP IF THIS IS NOT THE HOME PAGE
       ===================================================== */

    if (books.length === 0) {
        return;
    }


    /* =====================================================
       MAIN FILTER FUNCTION
       ===================================================== */

    function filterBooks() {

        // Get search text
        const searchText = searchInput
            ? searchInput.value.trim().toLowerCase()
            : "";


        let visibleBooks = 0;


        books.forEach(function (book) {

            /*
             * Your HTML structure is:
             *
             * .book-item
             *      └── .book-card
             *             ├── data-book-name
             *             ├── data-author
             *             └── data-genre
             */

            const card = book.querySelector(".book-card");


            // Get genre from book-item/card
            const bookGenre =
                (
                    book.getAttribute("data-genre") ||
                    card?.getAttribute("data-genre") ||
                    ""
                ).toLowerCase();


            // Get book name
            const bookName =
                (
                    card?.getAttribute("data-book-name") ||
                    ""
                ).toLowerCase();


            // Get author
            const author =
                (
                    card?.getAttribute("data-author") ||
                    ""
                ).toLowerCase();


            /* =================================================
               CHECK GENRE
               ================================================= */

            const genreMatches =
                selectedGenre === "all" ||
                bookGenre === selectedGenre.toLowerCase();


            /* =================================================
               CHECK SEARCH
               ================================================= */

            const searchMatches =
                searchText === "" ||
                bookName.includes(searchText) ||
                author.includes(searchText) ||
                bookGenre.includes(searchText);


            /* =================================================
               SHOW / HIDE BOOK
               ================================================= */

            if (genreMatches && searchMatches) {

                book.style.display = "";

                visibleBooks++;

            }
            else {

                book.style.display = "none";

            }

        });


        /* =====================================================
           EMPTY SEARCH RESULT MESSAGE
           ===================================================== */

        if (noBooksMessage) {

            noBooksMessage.style.display =
                visibleBooks === 0 ? "block" : "none";

        }


        /* =====================================================
           EMPTY GENRE RESULT MESSAGE
           ===================================================== */

        if (filterEmpty) {

            filterEmpty.style.display =
                visibleBooks === 0 ? "block" : "none";

        }

    }


    /* =========================================================
       GENRE BUTTONS
       ========================================================= */

    genreButtons.forEach(function (button) {

        button.addEventListener("click", function () {

            // Get selected genre
            selectedGenre =
                this.getAttribute("data-genre") || "all";


            /* Remove active from all buttons */

            genreButtons.forEach(function (btn) {

                btn.classList.remove("active");

            });


            /* Add active to clicked button */

            this.classList.add("active");


            /* Apply genre + search filter */

            filterBooks();

        });

    });


    /* =========================================================
       SEARCH WHILE TYPING
       ========================================================= */

    if (searchInput) {

        searchInput.addEventListener("input", function () {

            filterBooks();

        });

    }


    /* =========================================================
       SEARCH BUTTON
       ========================================================= */

    if (searchButton) {

        searchButton.addEventListener("click", function () {

            filterBooks();

        });

    }


    /* =========================================================
       PRESS ENTER TO SEARCH
       ========================================================= */

    if (searchInput) {

        searchInput.addEventListener("keydown", function (event) {

            if (event.key === "Enter") {

                event.preventDefault();

                filterBooks();

            }

        });

    }


    /* =========================================================
       INITIAL FILTER
       ========================================================= */

    filterBooks();

});