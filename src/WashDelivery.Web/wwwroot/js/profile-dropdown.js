document.addEventListener('DOMContentLoaded', function() {
    const button = document.getElementById('user-menu-button');
    const dropdown = document.getElementById('user-menu-dropdown');

    if (button && dropdown) {
        // Upewnij się, że dropdown jest w odpowiedniej pozycji
        function positionDropdown() {
            const buttonRect = button.getBoundingClientRect();
            const windowHeight = window.innerHeight;
            const dropdownHeight = dropdown.offsetHeight;

            // Sprawdź, czy jest miejsce na dole
            if (buttonRect.bottom + dropdownHeight > windowHeight) {
                // Jeśli nie ma miejsca na dole, pokaż dropdown nad przyciskiem
                dropdown.style.top = 'auto';
                dropdown.style.bottom = '100%';
                dropdown.style.marginTop = '0';
                dropdown.style.marginBottom = '0.5rem';
            } else {
                // Pokaż dropdown pod przyciskiem
                dropdown.style.top = '100%';
                dropdown.style.bottom = 'auto';
                dropdown.style.marginTop = '0.5rem';
                dropdown.style.marginBottom = '0';
            }
        }

        button.addEventListener('click', function(e) {
            e.stopPropagation(); // Zapobiega propagacji zdarzenia
            const isExpanded = button.getAttribute('aria-expanded') === 'true';
            
            button.setAttribute('aria-expanded', !isExpanded);
            
            if (!isExpanded) {
                dropdown.classList.remove('hidden');
                positionDropdown(); // Ustaw pozycję przed pokazaniem
                requestAnimationFrame(() => {
                    dropdown.classList.remove('opacity-0', 'scale-95');
                    dropdown.classList.add('opacity-100', 'scale-100');
                });
            } else {
                dropdown.classList.remove('opacity-100', 'scale-100');
                dropdown.classList.add('opacity-0', 'scale-95');
                setTimeout(() => {
                    dropdown.classList.add('hidden');
                }, 100);
            }
        });

        // Zamknij dropdown gdy klikniemy poza nim
        document.addEventListener('click', function(e) {
            if (!button.contains(e.target) && !dropdown.contains(e.target)) {
                button.setAttribute('aria-expanded', 'false');
                dropdown.classList.remove('opacity-100', 'scale-100');
                dropdown.classList.add('opacity-0', 'scale-95');
                setTimeout(() => {
                    dropdown.classList.add('hidden');
                }, 100);
            }
        });

        // Aktualizuj pozycję przy scrollowaniu i zmianie rozmiaru okna
        window.addEventListener('scroll', positionDropdown);
        window.addEventListener('resize', positionDropdown);
    }
}); 

function updateUserInfo(user) {
    document.querySelector('.user-name').textContent = `${user.firstName} ${user.lastName}`;
    document.querySelector('.user-role').innerHTML = `
        <span class="inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ring-1 ring-inset ${getRoleClass(user.role)}">
            ${user.roleDisplay}
        </span>
    `;
}

function getRoleClass(role) {
    switch (role) {
        case 'Admin':
            return 'bg-red-100 text-red-700 ring-red-600/20';
        case 'LaundryManager':
            return 'bg-emerald-100 text-emerald-700 ring-emerald-600/20';
        case 'LaundryWorker':
            return 'bg-green-100 text-green-700 ring-green-600/20';
        case 'Courier':
            return 'bg-orange-100 text-orange-700 ring-orange-600/20';
        case 'Customer':
            return 'bg-blue-100 text-blue-700 ring-blue-600/20';
        default:
            return 'bg-gray-100 text-gray-700 ring-gray-600/20';
    }
} 