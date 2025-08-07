# MyTown Design System
## UI/UX Design Patterns & Component Library

### Document Overview
This document outlines the design system, UI/UX patterns, and component guidelines for the MyTown application. The design follows a hyper-modern Material Design approach with flat design principles, focusing on clean aesthetics and excellent user experience across web and mobile platforms.

---

## Design Philosophy

### Core Principles
- **Flat Design**: No shadows, gradients, or 3D effects
- **Clean Aesthetics**: Minimalist approach with focus on content
- **Accessibility**: WCAG 2.1 AA compliance
- **Responsive**: Mobile-first design approach
- **Consistency**: Unified design language across platforms

### Design Goals
- Modern, professional appearance
- Excellent readability and usability
- Fast loading and smooth interactions
- Cross-platform consistency
- South African market appeal

---

## Color System

### Primary Color Palette
```css
/* Primary Colors */
--charcoal: #2f4858ff;        /* Primary dark - Headers, text */
--lapis-lazuli: #33658aff;    /* Primary blue - Buttons, links */
--carolina-blue: #86bbd8ff;   /* Secondary blue - Accents, highlights */
--hunyadi-yellow: #f6ae2dff;  /* Warning/Alert - CTAs, notifications */
--orange-pantone: #f26419ff;  /* Danger/Error - Errors, destructive actions */
```

### Color Usage Guidelines

#### Charcoal (#2f4858)
- **Primary text** (headings, body text)
- **Navigation elements**
- **Form labels**
- **Card headers**

#### Lapis Lazuli (#33658a)
- **Primary buttons**
- **Active navigation states**
- **Links**
- **Form focus states**

#### Carolina Blue (#86bbd8)
- **Secondary buttons**
- **Background accents**
- **Hover states**
- **Information highlights**

#### Hunyadi Yellow (#f6ae2d)
- **Call-to-action buttons**
- **Warning messages**
- **Featured content**
- **Success states**

#### Orange Pantone (#f26419)
- **Error messages**
- **Destructive actions**
- **Alert notifications**
- **Critical information**

### Neutral Colors
```css
/* Neutral Palette */
--white: #ffffff;
--light-gray: #f8f9fa;
--medium-gray: #e9ecef;
--dark-gray: #6c757d;
--black: #000000;
```

---

## Typography System

### Font Stack
```css
/* Primary Font */
font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;

/* Monospace Font */
font-family: 'JetBrains Mono', 'Fira Code', 'Consolas', 'Monaco', monospace;
```

### Type Scale
```css
/* Heading Scale */
--h1-size: 2.5rem;    /* 40px */
--h2-size: 2rem;      /* 32px */
--h3-size: 1.75rem;   /* 28px */
--h4-size: 1.5rem;    /* 24px */
--h5-size: 1.25rem;   /* 20px */
--h6-size: 1rem;      /* 16px */

/* Body Text */
--body-large: 1.125rem;   /* 18px */
--body-medium: 1rem;      /* 16px */
--body-small: 0.875rem;   /* 14px */
--body-xs: 0.75rem;       /* 12px */

/* Line Heights */
--line-height-tight: 1.2;
--line-height-normal: 1.5;
--line-height-relaxed: 1.75;
```

### Font Weights
```css
--font-light: 300;
--font-normal: 400;
--font-medium: 500;
--font-semibold: 600;
--font-bold: 700;
```

---

## Spacing System

### Base Unit: 8px Grid
```css
/* Spacing Scale */
--space-xs: 0.25rem;    /* 4px */
--space-sm: 0.5rem;     /* 8px */
--space-md: 1rem;       /* 16px */
--space-lg: 1.5rem;     /* 24px */
--space-xl: 2rem;       /* 32px */
--space-2xl: 3rem;      /* 48px */
--space-3xl: 4rem;      /* 64px */
```

### Component Spacing
- **Card padding**: 1.5rem (24px)
- **Button padding**: 0.75rem 1.5rem (12px 24px)
- **Form field spacing**: 1rem (16px)
- **Section margins**: 2rem (32px)

---

## Border Radius System

### Corner Radius Scale
```css
/* Border Radius */
--radius-sm: 0.25rem;   /* 4px */
--radius-md: 0.5rem;    /* 8px */
--radius-lg: 0.75rem;   /* 12px */
--radius-xl: 1rem;      /* 16px */
--radius-2xl: 1.5rem;   /* 24px */
--radius-full: 9999px;  /* Full circle */
```

### Usage Guidelines
- **Cards**: 12px (--radius-lg)
- **Buttons**: 8px (--radius-md)
- **Form inputs**: 8px (--radius-md)
- **Modals**: 16px (--radius-xl)
- **Avatars**: Full circle (--radius-full)

---

## Component Library

### 1. Buttons

#### Primary Button
```css
.btn-primary {
    background-color: var(--lapis-lazuli);
    color: white;
    border: none;
    border-radius: var(--radius-md);
    padding: 0.75rem 1.5rem;
    font-weight: var(--font-medium);
    transition: background-color 0.2s ease;
}

.btn-primary:hover {
    background-color: #2a5a7a;
}
```

#### Secondary Button
```css
.btn-secondary {
    background-color: var(--carolina-blue);
    color: var(--charcoal);
    border: none;
    border-radius: var(--radius-md);
    padding: 0.75rem 1.5rem;
    font-weight: var(--font-medium);
    transition: background-color 0.2s ease;
}

.btn-secondary:hover {
    background-color: #7aa8c8;
}
```

#### CTA Button
```css
.btn-cta {
    background-color: var(--hunyadi-yellow);
    color: var(--charcoal);
    border: none;
    border-radius: var(--radius-md);
    padding: 0.75rem 1.5rem;
    font-weight: var(--font-semibold);
    transition: background-color 0.2s ease;
}

.btn-cta:hover {
    background-color: #e69d1a;
}
```

### 2. Cards

#### Standard Card
```css
.card {
    background-color: white;
    border: 1px solid var(--medium-gray);
    border-radius: var(--radius-lg);
    padding: var(--space-lg);
    transition: border-color 0.2s ease;
}

.card:hover {
    border-color: var(--carolina-blue);
}
```

#### Featured Card
```css
.card-featured {
    background-color: white;
    border: 2px solid var(--hunyadi-yellow);
    border-radius: var(--radius-lg);
    padding: var(--space-lg);
}
```

### 3. Form Elements

#### Input Fields
```css
.form-input {
    border: 2px solid var(--medium-gray);
    border-radius: var(--radius-md);
    padding: 0.75rem 1rem;
    font-size: var(--body-medium);
    transition: border-color 0.2s ease;
}

.form-input:focus {
    outline: none;
    border-color: var(--lapis-lazuli);
}
```

#### Select Dropdowns
```css
.form-select {
    border: 2px solid var(--medium-gray);
    border-radius: var(--radius-md);
    padding: 0.75rem 1rem;
    background-color: white;
    font-size: var(--body-medium);
    transition: border-color 0.2s ease;
}

.form-select:focus {
    outline: none;
    border-color: var(--lapis-lazuli);
}
```

### 4. Navigation

#### Primary Navigation
```css
.nav-primary {
    background-color: white;
    border-bottom: 1px solid var(--medium-gray);
    padding: var(--space-md) 0;
}

.nav-link {
    color: var(--charcoal);
    text-decoration: none;
    padding: var(--space-sm) var(--space-md);
    border-radius: var(--radius-md);
    transition: background-color 0.2s ease;
}

.nav-link:hover,
.nav-link.active {
    background-color: var(--carolina-blue);
    color: var(--charcoal);
}
```

### 5. Alerts & Notifications

#### Success Alert
```css
.alert-success {
    background-color: #d4edda;
    border: 1px solid #c3e6cb;
    border-radius: var(--radius-md);
    padding: var(--space-md);
    color: #155724;
}
```

#### Warning Alert
```css
.alert-warning {
    background-color: #fff3cd;
    border: 1px solid #ffeaa7;
    border-radius: var(--radius-md);
    padding: var(--space-md);
    color: #856404;
}
```

#### Error Alert
```css
.alert-error {
    background-color: #f8d7da;
    border: 1px solid #f5c6cb;
    border-radius: var(--radius-md);
    padding: var(--space-md);
    color: #721c24;
}
```

### 6. Modals & Overlays

#### Modal Container
```css
.modal {
    background-color: white;
    border-radius: var(--radius-xl);
    padding: var(--space-xl);
    max-width: 500px;
    width: 90%;
    margin: 2rem auto;
}
```

#### Modal Overlay
```css
.modal-overlay {
    background-color: rgba(47, 72, 88, 0.5);
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
}
```

### 7. Accordions

#### Accordion Container
```css
.accordion {
    border: 1px solid var(--medium-gray);
    border-radius: var(--radius-lg);
    overflow: hidden;
}
```

#### Accordion Item
```css
.accordion-item {
    border-bottom: 1px solid var(--medium-gray);
}

.accordion-item:last-child {
    border-bottom: none;
}
```

#### Accordion Header
```css
.accordion-header {
    background-color: var(--light-gray);
    padding: var(--space-md);
    cursor: pointer;
    transition: background-color 0.2s ease;
}

.accordion-header:hover {
    background-color: var(--medium-gray);
}
```

#### Accordion Content
```css
.accordion-content {
    padding: var(--space-md);
    background-color: white;
}
```

---

## Layout Patterns

### 1. Grid System
```css
/* 12-Column Grid */
.grid {
    display: grid;
    gap: var(--space-md);
}

.grid-1 { grid-template-columns: repeat(1, 1fr); }
.grid-2 { grid-template-columns: repeat(2, 1fr); }
.grid-3 { grid-template-columns: repeat(3, 1fr); }
.grid-4 { grid-template-columns: repeat(4, 1fr); }
.grid-6 { grid-template-columns: repeat(6, 1fr); }
.grid-12 { grid-template-columns: repeat(12, 1fr); }

/* Responsive Grid */
@media (max-width: 768px) {
    .grid-2, .grid-3, .grid-4, .grid-6 {
        grid-template-columns: 1fr;
    }
}
```

### 2. Container System
```css
.container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 var(--space-md);
}

.container-sm {
    max-width: 640px;
    margin: 0 auto;
    padding: 0 var(--space-md);
}

.container-lg {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 var(--space-md);
}
```

### 3. Flexbox Utilities
```css
.flex { display: flex; }
.flex-col { flex-direction: column; }
.flex-row { flex-direction: row; }
.items-center { align-items: center; }
.items-start { align-items: flex-start; }
.items-end { align-items: flex-end; }
.justify-center { justify-content: center; }
.justify-between { justify-content: space-between; }
.justify-start { justify-content: flex-start; }
.justify-end { justify-content: flex-end; }
```

---

## Animation & Transitions

### Transition Guidelines
```css
/* Standard Transitions */
--transition-fast: 0.15s ease;
--transition-normal: 0.2s ease;
--transition-slow: 0.3s ease;

/* Hover Effects */
.hover-lift {
    transition: transform var(--transition-normal);
}

.hover-lift:hover {
    transform: translateY(-2px);
}

/* Fade Effects */
.fade-in {
    animation: fadeIn var(--transition-slow) ease-in;
}

@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}
```

---

## Responsive Design

### Breakpoints
```css
/* Mobile First Approach */
--breakpoint-sm: 640px;
--breakpoint-md: 768px;
--breakpoint-lg: 1024px;
--breakpoint-xl: 1280px;
--breakpoint-2xl: 1536px;

/* Media Queries */
@media (min-width: 640px) { /* Small devices */ }
@media (min-width: 768px) { /* Medium devices */ }
@media (min-width: 1024px) { /* Large devices */ }
@media (min-width: 1280px) { /* Extra large devices */ }
```

### Mobile-Specific Patterns
- **Touch targets**: Minimum 44px height
- **Gesture support**: Swipe, pinch, tap
- **Offline capability**: Progressive Web App features
- **Fast loading**: Optimized images and assets

---

## Accessibility Guidelines

### WCAG 2.1 AA Compliance
- **Color contrast**: Minimum 4.5:1 ratio
- **Focus indicators**: Visible focus states
- **Keyboard navigation**: Full keyboard accessibility
- **Screen reader support**: Proper ARIA labels
- **Text scaling**: Support for 200% zoom

### Accessibility Features
```css
/* Focus Styles */
.focus-visible {
    outline: 2px solid var(--lapis-lazuli);
    outline-offset: 2px;
}

/* Reduced Motion */
@media (prefers-reduced-motion: reduce) {
    * {
        animation-duration: 0.01ms !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0.01ms !important;
    }
}
```

---

## Icon System

### Icon Guidelines
- **Size**: 16px, 20px, 24px, 32px
- **Style**: Outlined icons preferred
- **Color**: Inherit from parent or use semantic colors
- **Library**: Feather Icons or Heroicons

### Icon Usage
```css
.icon {
    width: 1.25rem;
    height: 1.25rem;
    stroke: currentColor;
    stroke-width: 2;
    fill: none;
}

.icon-sm { width: 1rem; height: 1rem; }
.icon-lg { width: 1.5rem; height: 1.5rem; }
.icon-xl { width: 2rem; height: 2rem; }
```

---

## Implementation Guidelines

### CSS Organization
```css
/* 1. CSS Custom Properties (Variables) */
:root {
    /* Colors, spacing, typography */
}

/* 2. Base Styles */
* { /* Reset and base styles */ }
body { /* Typography and layout */ }

/* 3. Component Styles */
.btn { /* Button styles */ }
.card { /* Card styles */ }

/* 4. Utility Classes */
.flex { /* Utility classes */ }

/* 5. Responsive Styles */
@media (min-width: 768px) { /* Responsive adjustments */ }
```

### Naming Conventions
- **Components**: `.component-name`
- **Modifiers**: `.component-name--modifier`
- **States**: `.component-name.is-active`
- **Utilities**: `.u-utility-name`

### File Structure
```
wwwroot/css/
├── base/
│   ├── reset.css
│   ├── typography.css
│   └── variables.css
├── components/
│   ├── buttons.css
│   ├── cards.css
│   ├── forms.css
│   └── navigation.css
├── layouts/
│   ├── grid.css
│   ├── containers.css
│   └── utilities.css
└── main.css
```

---

## Testing & Validation

### Design Testing Checklist
- [ ] Color contrast meets WCAG 2.1 AA standards
- [ ] All interactive elements are keyboard accessible
- [ ] Components work across all target browsers
- [ ] Responsive design functions on all breakpoints
- [ ] Loading states and error states are designed
- [ ] Animation and transitions are smooth
- [ ] Touch targets meet minimum size requirements

### Browser Support
- **Chrome**: Latest 2 versions
- **Firefox**: Latest 2 versions
- **Safari**: Latest 2 versions
- **Edge**: Latest 2 versions
- **Mobile browsers**: iOS Safari, Chrome Mobile

---

*This design system serves as the foundation for consistent, accessible, and modern UI/UX across the MyTown application.*

**Document Version**: 1.0  
**Last Updated**: January 2025  
**Next Review**: Before major UI updates 