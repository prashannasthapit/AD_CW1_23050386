window.getPageHtmlWithStyles = function () {
    function inlineStyles(element) {
        const computed = getComputedStyle(element);
        for (let i = 0; i < computed.length; i++) {
            const prop = computed[i];
            element.style[prop] = computed.getPropertyValue(prop);
        }
        Array.from(element.children).forEach(child => inlineStyles(child));
    }

    const cloned = document.documentElement.cloneNode(true);
    inlineStyles(cloned);
    return '<!DOCTYPE html>' + cloned.outerHTML;
};
