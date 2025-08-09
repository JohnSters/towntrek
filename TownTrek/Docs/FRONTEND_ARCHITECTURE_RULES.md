# Frontend Architecture Rules

## Core Principles

### 1. **NO CSS OR JAVASCRIPT IN VIEW FILES**
- **Rule**: View files (.cshtml) should NEVER contain inline CSS styles or JavaScript code
- **Reason**: Maintains separation of concerns, improves maintainability, and enables proper caching
- **Enforcement**: All CSS and JavaScript must be in separate files

### 2. **File Organization Structure**

#### CSS Files
```
wwwroot/css/
├── foundation/           # Base styles, variables, reset
├── components/          # Reusable UI components
├── layouts/            # Layout-specific styles
├── features/           # Feature-specific styles
│   ├── business/
│   ├── profile/
│   ├── subscription/
│   └── [feature-name]/
└── entrypoints/        # Main CSS entry points
    ├── admin.css       # Client portal styles
    ├── public.css      # Public site styles
    └── site.css        # General site styles
```

#### JavaScript Files
```
wwwroot/js/
├── [feature-name].js   # Feature-specific JavaScript
├── client-admin.js     # Client portal functionality
└── site.js            # General site functionality
```

### 3. **CSS Guidelines**

#### Import Structure in Entrypoints
```css
/* Foundation first */
@import url('../foundation/variables.css');
@import url('../foundation/base.css');

/* Components */
@import url('../components/buttons.css');
@import url('../components/forms.css');

/* Layouts */
@import url('../layouts/admin-layout.css');

/* Features */
@import url('../features/profile/profile.css');
```

#### Naming Conventions
- Use BEM methodology where appropriate
- Use semantic class names
- Prefix feature-specific classes with feature name when needed

### 4. **JavaScript Guidelines**

#### Structure
- Use ES6 classes for complex functionality
- Use module pattern for organization
- Always use `document.addEventListener('DOMContentLoaded', ...)` for initialization
- Export classes to window object for potential reuse

#### Example Structure
```javascript
class FeatureManager {
    constructor() {
        this.init();
    }

    init() {
        // Initialization code
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new FeatureManager();
});

window.FeatureManager = FeatureManager;
```

### 5. **View File Guidelines**

#### What Views Should Contain
- HTML markup only
- Razor syntax for server-side rendering
- References to external CSS/JS files in @section Scripts

#### What Views Should NOT Contain
- `<style>` tags with CSS
- `<script>` tags with JavaScript code
- Inline style attributes (except for dynamic server-generated values)

#### Correct Script References
```razor
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script src="~/js/feature-name.js" asp-append-version="true"></script>
}
```

### 6. **Validation Checklist**

Before committing any view file, ensure:
- [ ] No `<style>` tags present
- [ ] No `<script>` tags with inline JavaScript
- [ ] No inline `style=""` attributes (unless server-generated)
- [ ] All JavaScript functionality is in separate .js files
- [ ] All CSS styling is in separate .css files
- [ ] Script references use `asp-append-version="true"`

### 7. **Exceptions**

The only acceptable inline styles are:
- Server-generated dynamic styles (e.g., `style="background-color: @Model.Color"`)
- Critical above-the-fold CSS that must be inline for performance (rare cases)

### 8. **Enforcement**

- Code reviews must check for violations of these rules
- Any view file containing CSS or JavaScript should be rejected
- Refactor existing violations when encountered

## Benefits of This Architecture

1. **Maintainability**: Easier to find and modify styles/scripts
2. **Performance**: Better caching and minification
3. **Reusability**: Styles and scripts can be shared across views
4. **Testing**: JavaScript can be unit tested separately
5. **Debugging**: Easier to debug with proper source maps
6. **Collaboration**: Frontend developers can work on CSS/JS without touching views

## Migration Strategy

When encountering existing violations:
1. Extract CSS to appropriate feature file
2. Extract JavaScript to separate .js file
3. Update view to reference external files
4. Test functionality remains intact
5. Update this documentation if new patterns emerge