# MyTown CSS Architecture

This directory contains the modular CSS architecture for the MyTown application, following the design system principles outlined in the business documentation.

## File Structure

```
wwwroot/css/
├── README.md           # This documentation file
├── variables.css       # Design system variables and tokens
├── base.css           # Base styles, reset, and global layout
├── buttons.css        # Button components and variants
├── cards.css          # Card components and hover effects
├── forms.css          # Form elements and validation styles
├── navigation.css     # Navigation bar and menu styles
├── footer.css         # Footer layout and styling
├── landing.css        # Landing page specific styles
└── site.css          # Main stylesheet with imports and global overrides
```

## Design System

### Variables (variables.css)
- **Colors**: Primary palette (charcoal, lapis-lazuli, carolina-blue, hunyadi-yellow, orange-pantone)
- **Typography**: Font sizes, weights, and line heights
- **Spacing**: 8px grid system with consistent spacing scale
- **Border Radius**: Consistent corner radius values
- **Transitions**: Standard animation timing
- **Breakpoints**: Responsive design breakpoints

### Base Styles (base.css)
- CSS reset and box-sizing
- Typography hierarchy
- Layout utilities (flexbox, grid)
- Container systems
- Accessibility focus states
- Reduced motion support

### Components

#### Buttons (buttons.css)
- Primary, secondary, and CTA button variants
- Size modifiers (regular, large)
- Hover effects and animations
- Responsive button styles

#### Cards (cards.css)
- Standard and featured card variants
- Hover effects and transitions
- Responsive card layouts

#### Forms (forms.css)
- Input and select styling
- Focus states and validation
- Form labels and groups
- Error and success states

#### Navigation (navigation.css)
- Navbar styling and responsive behavior
- Navigation links and hover states
- Mobile navigation support

#### Footer (footer.css)
- Footer layout with grid system
- Vertical link lists as requested
- Brand section with gradient text
- Responsive footer design

#### Landing Page (landing.css)
- Hero section with gradient background
- Feature cards and icons
- Stats section styling
- CTA section design
- Page-specific animations

## Usage

### In Layout Files
The CSS files are loaded in the following order in `_Layout.cshtml`:

1. Bootstrap (for base framework)
2. Variables (design tokens)
3. Base (reset and global styles)
4. Components (buttons, cards, forms, navigation, footer)
5. Page-specific styles (landing)
6. Site overrides (global customizations)

### Adding New Styles

#### For New Components
1. Create a new CSS file (e.g., `modals.css`)
2. Add component-specific styles
3. Include responsive design
4. Add the file to `_Layout.cshtml`
5. Update this README

#### For Page-Specific Styles
1. Create a new CSS file (e.g., `dashboard.css`)
2. Add page-specific styles
3. Include the file only on relevant pages
4. Document the new file here

## Best Practices

### CSS Organization
- Use CSS custom properties (variables) for consistency
- Follow BEM-like naming conventions
- Group related styles together
- Add comments for complex styles

### Responsive Design
- Mobile-first approach
- Use consistent breakpoints from variables
- Test on multiple screen sizes
- Consider touch targets on mobile

### Performance
- Minimize CSS file sizes
- Use efficient selectors
- Avoid deep nesting
- Leverage browser caching with asp-append-version

### Accessibility
- Maintain proper color contrast
- Include focus states for all interactive elements
- Support reduced motion preferences
- Use semantic HTML with appropriate styling

## Maintenance

### Regular Tasks
- Review and consolidate duplicate styles
- Update variables when design system changes
- Test responsive behavior on new features
- Validate accessibility compliance

### When Adding Features
- Check if existing components can be reused
- Follow established patterns and conventions
- Update documentation
- Test across browsers and devices

## Browser Support
- Chrome: Latest 2 versions
- Firefox: Latest 2 versions
- Safari: Latest 2 versions
- Edge: Latest 2 versions
- Mobile browsers: iOS Safari, Chrome Mobile

## South African Design Considerations
- Colors chosen to reflect South African aesthetic
- Typography optimized for local market
- Mobile-first approach for local device usage
- Performance optimized for varying connection speeds