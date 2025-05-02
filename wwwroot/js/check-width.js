// Script to check the width of the main container on ashergems.com
// This is just for reference and will be removed after we determine the width

document.addEventListener('DOMContentLoaded', function() {
    console.log('Checking container width...');
    
    // Create a div to display the width information
    const widthInfo = document.createElement('div');
    widthInfo.style.position = 'fixed';
    widthInfo.style.top = '10px';
    widthInfo.style.right = '10px';
    widthInfo.style.backgroundColor = 'rgba(0,0,0,0.7)';
    widthInfo.style.color = 'white';
    widthInfo.style.padding = '10px';
    widthInfo.style.zIndex = '9999';
    widthInfo.style.borderRadius = '5px';
    
    // Get the container width
    const containers = document.querySelectorAll('.container');
    let containerWidths = [];
    
    containers.forEach(container => {
        const computedStyle = window.getComputedStyle(container);
        const width = computedStyle.getPropertyValue('width');
        containerWidths.push(width);
    });
    
    // Display the width information
    widthInfo.textContent = `Container widths: ${containerWidths.join(', ')}`;
    document.body.appendChild(widthInfo);
    
    // Also log window width
    widthInfo.textContent += ` | Window width: ${window.innerWidth}px`;
    
    // Update on resize
    window.addEventListener('resize', function() {
        widthInfo.textContent = `Container widths: ${containerWidths.join(', ')} | Window width: ${window.innerWidth}px`;
    });
});
