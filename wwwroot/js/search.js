(() => {
    const searchInput = document.getElementById('searchInput');
    const suggestionBox = document.getElementById('searchSuggestions');

    if (!searchInput || !suggestionBox) return;

    const cakes = [
        { name: 'Chocolate Truffle Cake', url: '/Home/Cakes' },
        { name: 'Classic Birthday Cake', url: '/Home/Cakes' },
        { name: 'Vanilla Berry Cupcakes', url: '/Home/Cakes' },
        { name: 'Dark Cocoa Supreme', url: '/Home/Cakes' },
        { name: 'Confetti Party Cake', url: '/Home/Cakes' },
        { name: 'Classic Choco Cupcakes', url: '/Home/Cakes' }
    ];

    function hideSuggestions() {
        suggestionBox.innerHTML = '';
        suggestionBox.classList.remove('show');
    }

    function renderSuggestions(query) {
        if (!query) {
            hideSuggestions();
            return;
        }

        const q = query.toLowerCase();
        const results = cakes.filter(item => item.name.toLowerCase().includes(q)).slice(0, 6);

        if (!results.length) {
            suggestionBox.innerHTML = '<li class="search-empty">No cakes found</li>';
            suggestionBox.classList.add('show');
            return;
        }

        suggestionBox.innerHTML = results
            .map(item => `<li><a href="${item.url}" data-cake="${item.name}">${item.name}</a></li>`)
            .join('');
        suggestionBox.classList.add('show');
    }

    searchInput.addEventListener('keyup', (e) => {
        renderSuggestions(searchInput.value.trim());

        if (e.key === 'Enter') {
            const first = suggestionBox.querySelector('a');
            if (first) {
                const cakeName = first.dataset.cake;
                if (cakeName) {
                    localStorage.setItem('searchCakeQuery', cakeName);
                }
                window.location.href = first.getAttribute('href');
            }
        }
    });

    suggestionBox.addEventListener('click', (e) => {
        const link = e.target.closest('a');
        if (!link) return;

        const cakeName = link.dataset.cake;
        if (cakeName) {
            localStorage.setItem('searchCakeQuery', cakeName);
        }
        hideSuggestions();
    });

    document.addEventListener('click', (e) => {
        if (!e.target.closest('.search-box')) {
            hideSuggestions();
        }
    });
})();

