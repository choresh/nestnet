using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace NestNet.Infra.Swagger
{
    public static class SwaggerUICustomization
    {
        public static void ConfigureSwaggerUI(SwaggerUIOptions options)
        {
            // Customize Swagger UI options
            options.DocExpansion(DocExpansion.List);
            options.DefaultModelExpandDepth(2);
            options.DefaultModelsExpandDepth(-1);

            // Add custom initialization script
            options.HeadContent = @"
                <script>
                    function initializeMultiSelects() {

                        console.log('Initializing multi-selects...');
                        const selects = document.querySelectorAll('.swagger-ui select');

                        const paramRow = select.closest('tr');
                        const typeCell = paramRow?.querySelector('td.parameters-col_description');

                        console.log('Type cell content:', typeCell?.textContent);
                        
                        selects.forEach(select => {
                            // Look for array type indicator ('Available values') in the parameter row
                            const paramRow = select.closest('tr');
                            const typeCell = paramRow?.querySelector('td.parameters-col_description');          
                            if (typeCell?.textContent.includes('Available values')) {
                                
                                // Create unique identifier for this select
                                const uniqueId = `select-${Math.random().toString(36).substring(7)}`;
                                console.log('Created unique ID:', uniqueId);
                                select.setAttribute('data-select-id', uniqueId);
                                
                                // Skip if already processed
                                if (select.hasAttribute('data-processed')) {
                                    console.log('Select already processed, skipping');
                                    return;
                                }
                                
                                // Mark as processed
                                select.setAttribute('data-processed', 'true');
                                
                                // Create wrapper div
                                const wrapper = document.createElement('div');
                                wrapper.style.display = 'flex';
                                wrapper.style.flexDirection = 'column';
                                wrapper.style.gap = '5px';
                                wrapper.style.marginTop = '5px';
                                wrapper.style.maxWidth = 'fit-content';
                                
                                // Move select into wrapper
                                const parentCell = select.closest('td');
                                parentCell.insertBefore(wrapper, select);
                                wrapper.appendChild(select);
                                
                                // Create textbox container for textbox and clear button
                                const textboxContainer = document.createElement('div');
                                textboxContainer.style.display = 'flex';
                                textboxContainer.style.gap = '5px';
                                textboxContainer.style.alignItems = 'center';
                                wrapper.appendChild(textboxContainer);
                                
                                // Create textbox with unique identifier
                                const textbox = document.createElement('input');
                                textbox.type = 'text';
                                textbox.readOnly = true;
                                textbox.setAttribute('data-textbox-id', uniqueId);
                                console.log('Created textbox with ID:', uniqueId);
                                textbox.style.flex = '1';
                                textbox.style.padding = '5px';
                                textbox.style.border = '1px solid #ccc';
                                textbox.placeholder = 'Selected values will appear here';
                                textboxContainer.appendChild(textbox);
                                
                                // Add clear button next to textbox
                                const clearButton = document.createElement('button');
                                clearButton.textContent = 'Clear';
                                clearButton.style.whiteSpace = 'nowrap';
                                clearButton.onclick = function() {
                                     Array.from(select.options).forEach(opt => {
                                        opt.selected = false;
                                        opt.style.display = ''; // Show all options
                                    });
                                    textbox.value = ''; // Clear the accumulated values
                                    select.value = ''; // Clear the selected value
                                };
                                textboxContainer.appendChild(clearButton);
                                
                                // Do not Enable multiple selection
                                select.multiple = false;

                                // TODO: Temp patch!
                                const enableOnce = typeCell.textContent.includes('Property names');

                                // Function to update selected values
                                const updateSelectedValues = (sourceSelect) => {
                                    // Find the parent wrapper and then find the textbox within it
                                    const wrapper = sourceSelect.closest('div[style*=""flex-direction: column""]');
                                    if (!wrapper) {
                                        console.error('Wrapper not found for select');
                                        return;
                                    }
                                    
                                    const targetTextbox = wrapper.querySelector('input[type=""text""]');
                                    
                                    if (!targetTextbox) {
                                        console.error('Textbox not found in wrapper');
                                        return;
                                    }
                                    
                                    const selectedOption = sourceSelect.options[sourceSelect.selectedIndex];
                                    if (selectedOption && selectedOption.value) {
                                        targetTextbox.value += (targetTextbox.value ? ',' : '') + selectedOption.value;
                                        
                                        // Reset only this specific dropdown
                                        sourceSelect.selectedIndex = -1;

                                        if (enableOnce) {
                                            selectedOption.style.display = 'none';
                                        }
                                    }

                                    // Prevent event propagation
                                    event.stopPropagation();
                                };

                                // Handle selection changes
                                select.addEventListener('change', function(event) {
                                    event.stopPropagation();
                                    updateSelectedValues(this);
                                    return false;
                                });

                                // Prevent the change event from bubbling
                                select.addEventListener('click', function(event) {
                                    event.stopPropagation();
                                });

                                console.log('Processed select element:', select.name);
                            }
                        });
                    }

                    function waitForSwaggerUI() {
                        if (document.querySelector('.swagger-ui')) {
                            console.log('Swagger UI found, initializing...');
                            initializeMultiSelects();
                            
                            // Also watch for changes in the DOM
                            const observer = new MutationObserver((mutations) => {
                                mutations.forEach((mutation) => {
                                    if (mutation.addedNodes.length) {
                                        setTimeout(initializeMultiSelects, 100);
                                    }
                                });
                            });
                            
                            observer.observe(document.querySelector('.swagger-ui'), {
                                childList: true,
                                subtree: true
                            });
                        } else {
                            console.log('Swagger UI not found, retrying...');
                            setTimeout(waitForSwaggerUI, 500);
                        }
                    }

                    // Start the initialization process
                    if (document.readyState === 'loading') {
                        document.addEventListener('DOMContentLoaded', waitForSwaggerUI);
                    } else {
                        waitForSwaggerUI();
                    }

                    // Additional initialization attempts
                    setTimeout(waitForSwaggerUI, 1000);
                    setTimeout(waitForSwaggerUI, 2000);
                    setTimeout(waitForSwaggerUI, 3000);
                </script>

                <style>
                    .swagger-ui select[multiple] {
                        min-height: 100px !important;
                        width: auto !important;
                        min-width: 300px !important;
                    }
                    .swagger-ui input[readonly] {
                        background-color: #fafafa !important;
                        min-width: 300px !important;
                    }
                </style>
            ";
        }
    }
} 