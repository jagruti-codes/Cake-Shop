<script>    //cart script

    function addToCart(name, price){

        let cart = JSON.parse(localStorage.getItem("cart")) || [];

    cart.push({
        name:name,
    price:price
    });

    localStorage.setItem("cart",JSON.stringify(cart));

    alert("Added to Cart");

    }

</script>