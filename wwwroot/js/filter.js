<script> //filtter script

    let buttons = document.querySelectorAll(".filter-btn");
    let cakes = document.querySelectorAll(".cake-item");

    buttons.forEach(btn => {

        btn.addEventListener("click", () => {

            let filter = btn.getAttribute("data-filter");

            cakes.forEach(cake => {

                if (filter === "all" || cake.classList.contains(filter)) {
                    cake.style.display = "block";
                }
                else {
                    cake.style.display = "none";
                }

            });

        });

    });

</script>
