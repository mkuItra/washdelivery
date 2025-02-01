class WashDrawer extends HTMLElement {
    constructor() {
        super();
        this.attachShadow({ mode: 'open', delegatesFocus: true });
        
        this._isAnimating = false;
        
        this.shadowRoot.innerHTML = `
            <style>
                :host {
                    position: fixed;
                    inset: 0;
                    z-index: 50;
                    visibility: hidden;
                }

                .drawer {
                    position: fixed;
                    inset: 0;
                }

                .overlay {
                    position: fixed;
                    inset: 0;
                    background-color: rgb(17, 24, 39);
                    opacity: 0;
                    transition: opacity 0.15s ease-out;
                    cursor: pointer;
                }

                :host([open]) .overlay {
                    opacity: 0.75;
                }

                .content {
                    position: fixed;
                    bottom: 0;
                    left: 0;
                    right: 0;
                    background-color: white;
                    border-top-left-radius: 0.75rem;
                    border-top-right-radius: 0.75rem;
                    max-height: 90vh;
                    transform: translateY(100%);
                    transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
                }

                :host([open]) .content {
                    transform: translateY(0);
                }

                .handle {
                    height: 0.375rem;
                    width: 3rem;
                    background-color: rgb(209, 213, 219);
                    border-radius: 9999px;
                    margin: 0.75rem auto;
                }

                .header {
                    position: relative;
                    padding: 1rem;
                    margin: 0 auto;
                    max-width: 32rem;
                }

                .header-container {
                    border-bottom: 1px solid rgb(229, 231, 235);
                }

                .close-button {
                    position: absolute;
                    right: 1rem;
                    top: 50%;
                    transform: translateY(-50%);
                    padding: 0.5rem;
                    color: rgb(107, 114, 128);
                    cursor: pointer;
                    border: none;
                    background: none;
                    border-radius: 9999px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    transition: background-color 0.2s;
                }

                .close-button:hover {
                    background-color: rgb(243, 244, 246);
                }

                .close-button svg {
                    width: 1.25rem;
                    height: 1.25rem;
                }

                .title {
                    font-size: 1.125rem;
                    font-weight: 500;
                    color: rgb(17, 24, 39);
                    padding-right: 2.5rem;
                }

                .body {
                    margin-top: 0;
                    overflow-y: auto;
                }

                ::slotted(*) {
                    width: 100%;
                }
            </style>

            <div class="drawer">
                <div class="overlay" id="overlay"></div>
                <div class="content">
                    <div class="handle"></div>
                    <div class="header-container">
                        <div class="header">
                            <h2 class="title"><slot name="title">Drawer</slot></h2>
                            <button class="close-button" aria-label="Zamknij">
                                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                                </svg>
                            </button>
                        </div>
                    </div>
                    <div class="body">
                        <slot></slot>
                    </div>
                </div>
            </div>
        `;

        this.drawer = this.shadowRoot.querySelector('.drawer');
        this.overlay = this.shadowRoot.querySelector('.overlay');
        this.content = this.shadowRoot.querySelector('.content');
        this.closeButton = this.shadowRoot.querySelector('.close-button');
        this.setupEvents();
    }

    static get observedAttributes() {
        return ['open'];
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (name === 'open') {
            if (this._isAnimating) return;

            if (newValue !== null) {
                this.show();
            } else {
                this.hide();
            }
        }
    }

    setupEvents() {
        let touchStart = null;

        this.addEventListener('click', (e) => {
            const path = e.composedPath();
            if (path.includes(this.overlay) || path.includes(this.closeButton)) {
                this.hide();
            }
        });

        this.addEventListener('touchstart', (e) => {
            if (e.target.closest('.handle')) {
                touchStart = e.touches[0].clientY;
            }
        });

        this.addEventListener('touchmove', (e) => {
            if (!touchStart) return;
            const touchEnd = e.touches[0].clientY;
            const diff = touchEnd - touchStart;
            
            if (diff > 50) {
                this.hide();
            }
        });

        this.addEventListener('touchend', () => {
            touchStart = null;
        });
    }

    show() {
        console.log('Showing drawer');
        if (this.hasAttribute('open')) return;
        
        this._isAnimating = true;
        this.style.visibility = 'visible';
        requestAnimationFrame(() => {
            this.setAttribute('open', '');
            document.body.style.overflow = 'hidden';
            this._isAnimating = false;
        });
    }

    hide() {
        console.log('Hiding drawer');
        if (!this.hasAttribute('open')) return;
        
        this._isAnimating = true;
        requestAnimationFrame(() => {
            this.removeAttribute('open');
            document.body.style.overflow = '';
        });
        
        this.content.addEventListener('transitionend', () => {
            this.style.visibility = 'hidden';
            this._isAnimating = false;
        }, { once: true });
    }

    open() {
        if (!this.hasAttribute('open')) {
            this.show();
        }
    }

    close() {
        if (this.hasAttribute('open')) {
            this.hide();
        }
    }
}

customElements.define('wash-drawer', WashDrawer); 