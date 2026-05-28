(function() {
    var tocDiv = document.querySelector('.sphinx-toc');
    if (!tocDiv) return;

    // Sphinx wraps the full TOC in <ul><li><a href="#">Page Title</a><ul>…</ul></li></ul>.
    // The outer <li> repeats the page title already shown in the <h3> above.
    // Unwrap it so the real section links sit at the top level.
    var outerUl = tocDiv.querySelector(':scope > ul');
    if (outerUl) {
        var outerLi = outerUl.querySelector(':scope > li');
        if (outerLi) {
            var innerUl = outerLi.querySelector(':scope > ul');
            if (innerUl) {
                outerUl.parentNode.replaceChild(innerUl, outerUl);
            } else {
                // No subsections — remove the title anchor and leave bare
                var titleAnchor = outerLi.querySelector(':scope > a');
                if (titleAnchor) titleAnchor.remove();
            }
        }
    }

    // Reset bullet styling on all nested <ul> elements
    var uls = tocDiv.querySelectorAll('ul');
    for (var u = 0; u < uls.length; u++) {
        uls[u].classList.add('list-reset', 'pl-4');
    }

    // Replace browser bullet points with bold '-' characters to match the
    // MkDocs sidebar TOC pattern. Each <li> gets a flex row wrapper containing
    // the dash and the link; any nested <ul> falls below as a block element.
    var items = tocDiv.querySelectorAll('li');
    for (var i = 0; i < items.length; i++) {
        var li = items[i];
        var link = li.querySelector(':scope > a');
        if (link) {
            link.classList.add('break-words', 'max-w-full');
            var row = document.createElement('div');
            row.className = 'flex mb-1';
            var dash = document.createElement('span');
            dash.className = 'font-bold mr-2 flex-no-shrink';
            dash.textContent = '-';
            row.appendChild(dash);
            li.insertBefore(row, link);
            row.appendChild(link);
        }
    }
})();