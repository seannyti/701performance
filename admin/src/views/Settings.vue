<template>
  <AdminLayout>
    <div class="settings-page">
      <!-- Loading overlay -->
      <div v-if="isLoading && settings.length === 0" class="loading-overlay">
        <div class="spinner spinner-lg"></div>
      </div>

      <h1 class="page-title">Site Settings</h1>

      <!-- Tab Navigation -->
      <div class="tabs">
        <button 
          v-for="tab in tabs" 
          :key="tab.id"
          @click="activeTab = tab.id"
          :class="['tab', { active: activeTab === tab.id }]"
        >
          <span class="tab-icon">{{ tab.icon }}</span>
          {{ tab.label }}
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading settings...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="error-state">
        <p>{{ error }}</p>
        <div class="error-actions">
          <button @click="loadSettings" class="btn btn-secondary">Retry</button>
          <button 
            @click="resetSettings" 
            class="btn btn-danger"
            :disabled="isActionLoading('resetSettings')"
          >
            <span v-if="isActionLoading('resetSettings')" class="btn-spinner"></span>
            {{ isActionLoading('resetSettings') ? 'Resetting...' : 'Reset to Defaults' }}
          </button>
        </div>
      </div>

      <!-- Settings Form -->
      <div v-else class="settings-form">
        <!-- General Tab -->
        <div v-if="activeTab === 'general'" class="tab-content">
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('general-settings')">
              <h3 class="section-header">⚙️ General Settings</h3>
              <span class="collapse-icon">{{ expandedSections['general-settings'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['general-settings']" class="collapsible-content">
            <!-- Site Information Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">🏢 Site Information</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label required">Site Name</label>
                  <input 
                    v-model="getSetting('site_name').value" 
                    type="text" 
                    class="form-control"
                    required
                    placeholder="701 Performance Power"
                  />
                  <p class="form-hint">This appears in the browser tab and header</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Site Tagline</label>
                  <input 
                    v-model="getSetting('site_tagline').value" 
                    type="text" 
                    class="form-control"
                    placeholder="Your Ultimate Powersports Destination"
                  />
                  <p class="form-hint">A short description of your business</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Logo</label>
                  <div v-if="getSetting('logo_url').value" class="current-setting-image">
                    <img :src="getMediaUrl(getSetting('logo_url').value)" alt="Site Logo" style="max-height: 100px; background: white; padding: 10px;" />
                    <button @click="getSetting('logo_url').value = ''" type="button" class="btn btn-sm btn-danger">Remove</button>
                  </div>
                  <button @click="openMediaPickerFor('logo_url')" type="button" class="btn btn-secondary">
                    {{ getSetting('logo_url').value ? 'Change Logo' : 'Select from Library' }}
                  </button>
                  <p class="form-hint">Upload or select your site logo from the media library. Recommended: PNG with transparent background, minimum 200x60px</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Header Logo Height (px)</label>
                  <input 
                    v-model="getSetting('logo_header_height').value" 
                    type="number" 
                    class="form-control"
                    placeholder="40"
                    min="20"
                    max="100"
                  />
                  <p class="form-hint">Logo height in navigation bar (default: 40px). Range: 20-100px</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Footer Logo Height (px)</label>
                  <input 
                    v-model="getSetting('logo_footer_height').value" 
                    type="number" 
                    class="form-control"
                    placeholder="48"
                    min="30"
                    max="150"
                  />
                  <p class="form-hint">Logo height in footer section (default: 48px). Range: 30-150px</p>
                </div>
              </div>
            </div>

            <!-- Contact Information Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📞 Contact Information ("Get in Touch" Section)</h4>
              
              <!-- Core Contact Information -->
              <div class="settings-grid" style="margin-bottom: 1.5rem; padding-bottom: 1.5rem; border-bottom: 1px solid #e5e7eb;">
                <div class="form-group">
                  <label class="form-label required">Contact Email</label>
                  <input 
                    v-model="getSetting('contact_email').value" 
                    type="email" 
                    class="form-control"
                    required
                    placeholder="info@701performancepower.com"
                  />
                  <p class="form-hint">Main contact email for the business</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Contact Phone</label>
                  <input 
                    v-model="getSetting('contact_phone').value" 
                    type="tel" 
                    class="form-control"
                    placeholder="(701) 555-0100"
                  />
                  <p class="form-hint">Business phone number</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Contact Address</label>
                  <textarea 
                    v-model="getSetting('contact_address').value" 
                    class="form-control"
                    rows="2"
                    placeholder="123 Powersports Drive, Fargo, ND 58102"
                  ></textarea>
                  <p class="form-hint">Physical business address</p>
                </div>
              </div>

              <!-- Contact Cards Section -->
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Contact Section Heading</label>
                  <input 
                    v-model="getSetting('contact_section_title').value" 
                    type="text" 
                    class="form-control"
                    placeholder="Get in Touch"
                  />
                  <p class="form-hint">Main title above all contact cards</p>
                </div>

                <!-- Address Card -->
                <div class="form-group full-width" style="background: #f9fafb; padding: 1rem; border-radius: 0.5rem; margin-bottom: 1rem;">
                  <h5 style="margin: 0 0 1rem 0; font-size: 0.875rem; font-weight: 600; color: #374151;">📍 Address Card</h5>
                  <div class="settings-grid">
                    <div class="form-group">
                      <label class="form-label">Card Title</label>
                      <input 
                        v-model="getSetting('contact_address_title').value" 
                        type="text" 
                        class="form-control"
                        placeholder="Visit Our Showroom"
                      />
                    </div>
                    <div class="form-group">
                      <label class="form-label">Note Text</label>
                      <input 
                        v-model="getSetting('contact_address_note').value" 
                        type="text" 
                        class="form-control"
                        placeholder="Open for personal consultations"
                      />
                    </div>
                  </div>
                </div>

                <!-- Phone Card -->
                <div class="form-group full-width" style="background: #f9fafb; padding: 1rem; border-radius: 0.5rem; margin-bottom: 1rem;">
                  <h5 style="margin: 0 0 1rem 0; font-size: 0.875rem; font-weight: 600; color: #374151;">📞 Phone Card</h5>
                  <div class="settings-grid">
                    <div class="form-group">
                      <label class="form-label">Card Title</label>
                      <input 
                        v-model="getSetting('contact_phone_title').value" 
                        type="text" 
                        class="form-control"
                        placeholder="Call Us"
                      />
                    </div>
                    <div class="form-group full-width">
                      <label class="form-label">Business Hours</label>
                      <textarea 
                        v-model="getSetting('contact_hours').value" 
                        class="form-control"
                        rows="2"
                        placeholder="Mon-Fri: 9AM-6PM&#10;Sat: 9AM-4PM"
                      ></textarea>
                    </div>
                  </div>
                </div>

                <!-- Email Card -->
                <div class="form-group full-width" style="background: #f9fafb; padding: 1rem; border-radius: 0.5rem; margin-bottom: 1rem;">
                  <h5 style="margin: 0 0 1rem 0; font-size: 0.875rem; font-weight: 600; color: #374151;">✉️ Email Card</h5>
                  <div class="settings-grid">
                    <div class="form-group">
                      <label class="form-label">Card Title</label>
                      <input 
                        v-model="getSetting('contact_email_title').value" 
                        type="text" 
                        class="form-control"
                        placeholder="Email Us"
                      />
                    </div>
                    <div class="form-group">
                      <label class="form-label">Response Note</label>
                      <input 
                        v-model="getSetting('contact_email_note').value" 
                        type="text" 
                        class="form-control"
                        placeholder="We respond within 24 hours"
                      />
                    </div>
                  </div>
                </div>

                <!-- Live Chat Card -->
                <div class="form-group full-width" style="background: #f9fafb; padding: 1rem; border-radius: 0.5rem;">
                  <h5 style="margin: 0 0 1rem 0; font-size: 0.875rem; font-weight: 600; color: #374151;">💬 Live Chat Card</h5>
                  <div class="settings-grid">
                    <div class="form-group">
                      <label class="form-label">Card Title</label>
                      <input 
                        v-model="getSetting('contact_livechat_title').value" 
                        type="text" 
                        class="form-control"
                        placeholder="Live Chat"
                      />
                    </div>
                    <div class="form-group">
                      <label class="form-label">Availability Text</label>
                      <input 
                        v-model="getSetting('contact_livechat_text').value" 
                        type="text" 
                        class="form-control"
                        placeholder="Available during business hours"
                      />
                    </div>
                    <div class="form-group full-width">
                      <label class="form-label">Note Text</label>
                      <input 
                        v-model="getSetting('contact_livechat_note').value" 
                        type="text" 
                        class="form-control"
                        placeholder="Quick answers to your questions"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Save Button for General -->
            <div class="form-actions">
              <button @click="saveSection(['site_name', 'site_tagline', 'logo_url', 'logo_header_height', 'logo_footer_height', 'contact_email', 'contact_phone', 'contact_address', 'contact_section_title', 'contact_address_title', 'contact_phone_title', 'contact_email_title', 'contact_livechat_title', 'contact_hours', 'contact_address_note', 'contact_email_note', 'contact_livechat_text', 'contact_livechat_note'], 'general')" class="btn btn-primary btn-lg" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save General Settings' }}
              </button>
            </div>
            </div> <!-- End collapsible-content -->
          </div>
        </div>

        <!-- Theme & Design Tab -->
        <div v-if="activeTab === 'theme'" class="tab-content">
          
          <!-- Theme Presets Section -->
          <div class="settings-card theme-presets-section">
            <div class="section-header-collapsible" @click="toggleSection('theme-presets')">
              <h3 class="section-header">✨ Quick Theme Presets</h3>
              <span class="collapse-icon">{{ expandedSections['theme-presets'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['theme-presets']" class="collapsible-content">
            <p class="section-description">Choose a pre-configured theme to instantly transform your site's appearance. You can further customize any preset after applying it.</p>
            
            <!-- Preset Category Tabs -->
            <div class="preset-tabs">
              <button 
                class="preset-tab" 
                :class="{ active: activePresetTab === 'general' }"
                @click="activePresetTab = 'general'"
              >
                🎨 General Themes
              </button>
              <button 
                class="preset-tab" 
                :class="{ active: activePresetTab === 'motorsports' }"
                @click="activePresetTab = 'motorsports'"
              >
                🏍️ Motorsports Themes
              </button>
            </div>

            <div class="theme-presets-grid">
              <div 
                v-for="preset in filteredPresets" 
                :key="preset.name"
                class="theme-preset-card"
                :class="{ active: isPresetActive(preset) }"
                @click="applyThemePreset(preset)"
              >
                <div class="preset-active-badge" v-if="isPresetActive(preset)">
                  ✓ Active
                </div>
                <div class="preset-icon">{{ preset.icon }}</div>
                <h4 class="preset-name">{{ preset.name }}</h4>
                <p class="preset-description">{{ preset.description }}</p>
                <div class="preset-preview-colors">
                  <div 
                    class="preview-color-dot" 
                    :style="{ backgroundColor: preset.preview.primary }"
                    :title="`Primary: ${preset.preview.primary}`"
                  ></div>
                  <div 
                    class="preview-color-dot" 
                    :style="{ backgroundColor: preset.preview.secondary }"
                    :title="`Secondary: ${preset.preview.secondary}`"
                  ></div>
                  <div 
                    class="preview-color-dot" 
                    :style="{ backgroundColor: preset.preview.bg }"
                    :title="`Background: ${preset.preview.bg}`"
                  ></div>
                </div>
                <div class="preset-apply-overlay">
                  <span class="icon">✨</span> Apply Theme
                </div>
              </div>
            </div>
            </div> <!-- End collapsible-content -->
          </div>

          <!-- Color Scheme Section -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('color-palette')">
              <h3 class="section-header">🎨 Color Palette</h3>
              <span class="collapse-icon">{{ expandedSections['color-palette'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['color-palette']" class="collapsible-content">
            <!-- Primary Colors -->
            <div class="subsection">
              <h4 class="subsection-title">Brand Colors</h4>
              <div class="settings-grid color-grid">
                <div class="form-group">
                  <label class="form-label">Primary Color</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_primary_color').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_primary_color').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#6366f1"
                    />
                  </div>
                  <p class="form-hint">Main brand color for buttons, links, and accents</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Secondary Color</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_secondary_color').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_secondary_color').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#ec4899"
                    />
                  </div>
                  <p class="form-hint">Secondary accent color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Accent Color</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_accent_color').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_accent_color').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#f59e0b"
                    />
                  </div>
                  <p class="form-hint">Highlight and call-to-action color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Success Color</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_success_color').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_success_color').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#10b981"
                    />
                  </div>
                  <p class="form-hint">Positive status and confirmation color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Warning Color</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_warning_color').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_warning_color').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#f59e0b"
                    />
                  </div>
                  <p class="form-hint">Warning and caution color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Danger Color</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_danger_color').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_danger_color').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#ef4444"
                    />
                  </div>
                  <p class="form-hint">Error and destructive action color</p>
                </div>
              </div>
            </div>

            <!-- Background Colors -->
            <div class="subsection">
              <h4 class="subsection-title">Background Colors</h4>
              <div class="settings-grid color-grid">
                <div class="form-group">
                  <label class="form-label">Page Background</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_bg_color').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_bg_color').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#ffffff"
                    />
                  </div>
                  <p class="form-hint">Main page background color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Secondary Background</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_bg_secondary').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_bg_secondary').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#f9fafb"
                    />
                  </div>
                  <p class="form-hint">Cards and section backgrounds</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Muted Background</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_bg_muted').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_bg_muted').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#f3f4f6"
                    />
                  </div>
                  <p class="form-hint">Subtle backgrounds and hover states</p>
                </div>
              </div>
            </div>

            <!-- Text Colors -->
            <div class="subsection">
              <h4 class="subsection-title">Text Colors</h4>
              <div class="settings-grid color-grid">
                <div class="form-group">
                  <label class="form-label">Primary Text</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_text_primary').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_text_primary').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#111827"
                    />
                  </div>
                  <p class="form-hint">Main body text color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Secondary Text</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_text_secondary').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_text_secondary').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#6b7280"
                    />
                  </div>
                  <p class="form-hint">Descriptions and less prominent text</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Muted Text</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_text_muted').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_text_muted').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#9ca3af"
                    />
                  </div>
                  <p class="form-hint">Hints and subtle information</p>
                </div>
              </div>
            </div>

            <!-- Border Colors -->
            <div class="subsection">
              <h4 class="subsection-title">Border Colors</h4>
              <div class="settings-grid color-grid">
                <div class="form-group">
                  <label class="form-label">Default Border</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_border_color').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_border_color').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#e5e7eb"
                    />
                  </div>
                  <p class="form-hint">Standard border color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Accent Border</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_border_accent').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_border_accent').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#d1d5db"
                    />
                  </div>
                  <p class="form-hint">Highlighted border color</p>
                </div>
              </div>
            </div>

            <button @click="saveSection(['theme_primary_color', 'theme_secondary_color', 'theme_accent_color', 'theme_success_color', 'theme_warning_color', 'theme_danger_color', 'theme_bg_color', 'theme_bg_secondary', 'theme_bg_muted', 'theme_text_primary', 'theme_text_secondary', 'theme_text_muted', 'theme_border_color', 'theme_border_accent'], 'theme')" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : 'Save Color Scheme' }}
            </button>
          </div>
          </div>

          <!-- Typography Section -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('typography')">
              <h3 class="section-header">✍️ Typography</h3>
              <span class="collapse-icon">{{ expandedSections['typography'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['typography']" class="collapsible-content">
            <!-- Font Families -->
            <div class="subsection">
              <h4 class="subsection-title">Font Families</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Heading Font</label>
                  <select v-model="getSetting('theme_font_heading').value" class="form-control">
                    <option value="'Inter', system-ui, sans-serif">Inter (Default)</option>
                    <option value="'Roboto', sans-serif">Roboto</option>
                    <option value="'Poppins', sans-serif">Poppins</option>
                    <option value="'Montserrat', sans-serif">Montserrat</option>
                    <option value="'Open Sans', sans-serif">Open Sans</option>
                    <option value="'Lato', sans-serif">Lato</option>
                    <option value="'Raleway', sans-serif">Raleway</option>
                    <option value="'Playfair Display', serif">Playfair Display</option>
                    <option value="'Merriweather', serif">Merriweather</option>
                    <option value="Georgia, serif">Georgia</option>
                  </select>
                  <p class="form-hint">Font for headings (H1-H6)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Body Font</label>
                  <select v-model="getSetting('theme_font_body').value" class="form-control">
                    <option value="'Inter', system-ui, sans-serif">Inter (Default)</option>
                    <option value="'Roboto', sans-serif">Roboto</option>
                    <option value="'Open Sans', sans-serif">Open Sans</option>
                    <option value="'Lato', sans-serif">Lato</option>
                    <option value="'Source Sans Pro', sans-serif">Source Sans Pro</option>
                    <option value="'Nunito', sans-serif">Nunito</option>
                    <option value="Georgia, serif">Georgia</option>
                    <option value="system-ui, sans-serif">System UI</option>
                  </select>
                  <p class="form-hint">Font for body text and paragraphs</p>
                </div>
              </div>
            </div>

            <!-- Font Sizes -->
            <div class="subsection">
              <h4 class="subsection-title">Font Sizes</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Base Font Size (px)</label>
                  <input 
                    v-model="getSetting('theme_font_size_base').value" 
                    type="number" 
                    class="form-control"
                    placeholder="16"
                    min="12"
                    max="24"
                  />
                  <p class="form-hint">Base font size (12-24px, default: 16px)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">H1 Size (rem)</label>
                  <input 
                    v-model="getSetting('theme_font_size_h1').value" 
                    type="number" 
                    step="0.1"
                    class="form-control"
                    placeholder="2.5"
                    min="1.5"
                    max="5"
                  />
                  <p class="form-hint">Heading 1 size in rem units</p>
                </div>

                <div class="form-group">
                  <label class="form-label">H2 Size (rem)</label>
                  <input 
                    v-model="getSetting('theme_font_size_h2').value" 
                    type="number" 
                    step="0.1"
                    class="form-control"
                    placeholder="2"
                    min="1.2"
                    max="4"
                  />
                  <p class="form-hint">Heading 2 size in rem units</p>
                </div>

                <div class="form-group">
                  <label class="form-label">H3 Size (rem)</label>
                  <input 
                    v-model="getSetting('theme_font_size_h3').value" 
                    type="number" 
                    step="0.1"
                    class="form-control"
                    placeholder="1.5"
                    min="1"
                    max="3"
                  />
                  <p class="form-hint">Heading 3 size in rem units</p>
                </div>
              </div>
            </div>

            <!-- Font Weights -->
            <div class="subsection">
              <h4 class="subsection-title">Font Weights</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Heading Weight</label>
                  <select v-model="getSetting('theme_font_weight_heading').value" class="form-control">
                    <option value="400">Regular (400)</option>
                    <option value="500">Medium (500)</option>
                    <option value="600">Semi-Bold (600)</option>
                    <option value="700">Bold (700)</option>
                    <option value="800">Extra-Bold (800)</option>
                    <option value="900">Black (900)</option>
                  </select>
                  <p class="form-hint">Weight for headings</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Body Weight</label>
                  <select v-model="getSetting('theme_font_weight_body').value" class="form-control">
                    <option value="300">Light (300)</option>
                    <option value="400">Regular (400)</option>
                    <option value="500">Medium (500)</option>
                    <option value="600">Semi-Bold (600)</option>
                  </select>
                  <p class="form-hint">Weight for body text</p>
                </div>
              </div>
            </div>

            <!-- Line Heights -->
            <div class="subsection">
              <h4 class="subsection-title">Line Heights</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Heading Line Height</label>
                  <input 
                    v-model="getSetting('theme_line_height_heading').value" 
                    type="number" 
                    step="0.1"
                    class="form-control"
                    placeholder="1.2"
                    min="1"
                    max="2"
                  />
                  <p class="form-hint">Line height for headings (1.0-2.0)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Body Line Height</label>
                  <input 
                    v-model="getSetting('theme_line_height_body').value" 
                    type="number" 
                    step="0.1"
                    class="form-control"
                    placeholder="1.6"
                    min="1.2"
                    max="2.5"
                  />
                  <p class="form-hint">Line height for body text (1.2-2.5)</p>
                </div>
              </div>
            </div>

            <button @click="saveSection(['theme_font_heading', 'theme_font_body', 'theme_font_size_base', 'theme_font_size_h1', 'theme_font_size_h2', 'theme_font_size_h3', 'theme_font_weight_heading', 'theme_font_weight_body', 'theme_line_height_heading', 'theme_line_height_body'], 'theme')" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : 'Save Typography' }}
            </button>
            </div>
          </div>

          <!-- Component Styling Section -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('components')">
              <h3 class="section-header">🎯 Component Styling</h3>
              <span class="collapse-icon">{{ expandedSections['components'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['components']" class="collapsible-content">
            <!-- Button Styles -->
            <div class="subsection">
              <h4 class="subsection-title">Buttons</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Button Border Radius (px)</label>
                  <input 
                    v-model="getSetting('theme_button_radius').value" 
                    type="number" 
                    class="form-control"
                    placeholder="6"
                    min="0"
                    max="50"
                  />
                  <p class="form-hint">Roundness of buttons (0-50px)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Button Padding (vertical)</label>
                  <input 
                    v-model="getSetting('theme_button_padding_y').value" 
                    type="number" 
                    step="0.25"
                    class="form-control"
                    placeholder="0.75"
                    min="0.25"
                    max="2"
                  />
                  <p class="form-hint">Vertical padding in rem (0.25-2)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Button Padding (horizontal)</label>
                  <input 
                    v-model="getSetting('theme_button_padding_x').value" 
                    type="number" 
                    step="0.25"
                    class="form-control"
                    placeholder="1.5"
                    min="0.5"
                    max="3"
                  />
                  <p class="form-hint">Horizontal padding in rem (0.5-3)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Button Font Weight</label>
                  <select v-model="getSetting('theme_button_font_weight').value" class="form-control">
                    <option value="400">Regular (400)</option>
                    <option value="500">Medium (500)</option>
                    <option value="600">Semi-Bold (600)</option>
                    <option value="700">Bold (700)</option>
                  </select>
                  <p class="form-hint">Text weight for buttons</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Button Text Transform</label>
                  <select v-model="getSetting('theme_button_text_transform').value" class="form-control">
                    <option value="none">Normal</option>
                    <option value="uppercase">UPPERCASE</option>
                    <option value="lowercase">lowercase</option>
                    <option value="capitalize">Capitalize First Letter</option>
                  </select>
                  <p class="form-hint">Text transformation for buttons</p>
                </div>
              </div>
            </div>

            <!-- Card Styles -->
            <div class="subsection">
              <h4 class="subsection-title">Cards & Containers</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Card Border Radius (px)</label>
                  <input 
                    v-model="getSetting('theme_card_radius').value" 
                    type="number" 
                    class="form-control"
                    placeholder="12"
                    min="0"
                    max="50"
                  />
                  <p class="form-hint">Roundness of cards (0-50px)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Card Padding (rem)</label>
                  <input 
                    v-model="getSetting('theme_card_padding').value" 
                    type="number" 
                    step="0.25"
                    class="form-control"
                    placeholder="1.5"
                    min="0.5"
                    max="4"
                  />
                  <p class="form-hint">Inner padding for cards (0.5-4 rem)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Card Shadow</label>
                  <select v-model="getSetting('theme_card_shadow').value" class="form-control">
                    <option value="none">None</option>
                    <option value="0 1px 3px 0 rgb(0 0 0 / 0.1)">Small</option>
                    <option value="0 4px 6px -1px rgb(0 0 0 / 0.1)">Medium</option>
                    <option value="0 10px 15px -3px rgb(0 0 0 / 0.1)">Large</option>
                    <option value="0 20px 25px -5px rgb(0 0 0 / 0.1)">Extra Large</option>
                  </select>
                  <p class="form-hint">Drop shadow for cards</p>
                </div>
              </div>
            </div>

            <!-- Input Styles -->
            <div class="subsection">
              <h4 class="subsection-title">Form Inputs</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Input Border Radius (px)</label>
                  <input 
                    v-model="getSetting('theme_input_radius').value" 
                    type="number" 
                    class="form-control"
                    placeholder="6"
                    min="0"
                    max="50"
                  />
                  <p class="form-hint">Roundness of inputs (0-50px)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Input Border Width (px)</label>
                  <input 
                    v-model="getSetting('theme_input_border_width').value" 
                    type="number" 
                    class="form-control"
                    placeholder="1"
                    min="0"
                    max="5"
                  />
                  <p class="form-hint">Border thickness for inputs (0-5px)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Input Focus Color</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_input_focus_color').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_input_focus_color').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#6366f1"
                    />
                  </div>
                  <p class="form-hint">Border color when input is focused</p>
                </div>
              </div>
            </div>

            <button @click="saveSection(['theme_button_radius', 'theme_button_padding_y', 'theme_button_padding_x', 'theme_button_font_weight', 'theme_button_text_transform', 'theme_card_radius', 'theme_card_padding', 'theme_card_shadow', 'theme_input_radius', 'theme_input_border_width', 'theme_input_focus_color'], 'theme')" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : 'Save Component Styles' }}
            </button>
            </div>
          </div>

          <!-- Layout & Spacing Section -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('layout')">
              <h3 class="section-header">📐 Layout & Spacing</h3>
              <span class="collapse-icon">{{ expandedSections['layout'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['layout']" class="collapsible-content">
            <div class="subsection">
              <h4 class="subsection-title">Container Widths</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Max Container Width (px)</label>
                  <input 
                    v-model="getSetting('theme_container_max_width').value" 
                    type="number" 
                    class="form-control"
                    placeholder="1280"
                    min="960"
                    max="1920"
                  />
                  <p class="form-hint">Maximum width of content container (960-1920px)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Container Padding (px)</label>
                  <input 
                    v-model="getSetting('theme_container_padding').value" 
                    type="number" 
                    class="form-control"
                    placeholder="20"
                    min="0"
                    max="100"
                  />
                  <p class="form-hint">Side padding for containers (0-100px)</p>
                </div>
              </div>
            </div>

            <div class="subsection">
              <h4 class="subsection-title">Section Spacing</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Section Padding Top (rem)</label>
                  <input 
                    v-model="getSetting('theme_section_padding_top').value" 
                    type="number" 
                    step="0.5"
                    class="form-control"
                    placeholder="4"
                    min="0"
                    max="10"
                  />
                  <p class="form-hint">Top padding for page sections (0-10 rem)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Section Padding Bottom (rem)</label>
                  <input 
                    v-model="getSetting('theme_section_padding_bottom').value" 
                    type="number" 
                    step="0.5"
                    class="form-control"
                    placeholder="4"
                    min="0"
                    max="10"
                  />
                  <p class="form-hint">Bottom padding for page sections (0-10 rem)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Element Gap (rem)</label>
                  <input 
                    v-model="getSetting('theme_element_gap').value" 
                    type="number" 
                    step="0.25"
                    class="form-control"
                    placeholder="1.5"
                    min="0.5"
                    max="4"
                  />
                  <p class="form-hint">Default spacing between elements (0.5-4 rem)</p>
                </div>
              </div>
            </div>

            <button @click="saveSection(['theme_container_max_width', 'theme_container_padding', 'theme_section_padding_top', 'theme_section_padding_bottom', 'theme_element_gap'], 'theme')" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : 'Save Layout & Spacing' }}
            </button>
            </div>
          </div>

          <!-- Effects & Animations Section -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('effects')">
              <h3 class="section-header">✨ Effects & Animations</h3>
              <span class="collapse-icon">{{ expandedSections['effects'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['effects']" class="collapsible-content">
            <div class="subsection">
              <h4 class="subsection-title">Transitions</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Transition Duration (ms)</label>
                  <input 
                    v-model="getSetting('theme_transition_duration').value" 
                    type="number" 
                    step="50"
                    class="form-control"
                    placeholder="200"
                    min="0"
                    max="1000"
                  />
                  <p class="form-hint">Duration of hover/focus transitions (0-1000ms)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Transition Timing</label>
                  <select v-model="getSetting('theme_transition_timing').value" class="form-control">
                    <option value="ease">Ease (Default)</option>
                    <option value="linear">Linear</option>
                    <option value="ease-in">Ease In</option>
                    <option value="ease-out">Ease Out</option>
                    <option value="ease-in-out">Ease In-Out</option>
                    <option value="cubic-bezier(0.4, 0, 0.2, 1)">Smooth</option>
                  </select>
                  <p class="form-hint">Animation timing function</p>
                </div>
              </div>
            </div>

            <div class="subsection">
              <h4 class="subsection-title">Hover Effects</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Enable Hover Lift</label>
                  <select v-model="getSetting('theme_hover_lift_enabled').value" class="form-control">
                    <option value="true">Enabled</option>
                    <option value="false">Disabled</option>
                  </select>
                  <p class="form-hint">Cards lift up slightly on hover</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Hover Lift Amount (px)</label>
                  <input 
                    v-model="getSetting('theme_hover_lift_amount').value" 
                    type="number" 
                    class="form-control"
                    placeholder="4"
                    min="0"
                    max="20"
                  />
                  <p class="form-hint">How much elements lift on hover (0-20px)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Hover Scale</label>
                  <input 
                    v-model="getSetting('theme_hover_scale').value" 
                    type="number" 
                    step="0.01"
                    class="form-control"
                    placeholder="1.02"
                    min="1"
                    max="1.2"
                  />
                  <p class="form-hint">Scale multiplier on hover (1.0-1.2)</p>
                </div>
              </div>
            </div>

            <div class="subsection">
              <h4 class="subsection-title">Shadow Effects</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Hover Shadow Intensity</label>
                  <select v-model="getSetting('theme_hover_shadow').value" class="form-control">
                    <option value="none">None</option>
                    <option value="0 4px 12px 0 rgb(0 0 0 / 0.1)">Subtle</option>
                    <option value="0 8px 20px 0 rgb(0 0 0 / 0.15)">Medium</option>
                    <option value="0 12px 28px 0 rgb(0 0 0 / 0.2)">Strong</option>
                    <option value="0 20px 40px 0 rgb(0 0 0 / 0.25)">Dramatic</option>
                  </select>
                  <p class="form-hint">Shadow that appears on hover</p>
                </div>
              </div>
            </div>

            <button @click="saveSection(['theme_transition_duration', 'theme_transition_timing', 'theme_hover_lift_enabled', 'theme_hover_lift_amount', 'theme_hover_scale', 'theme_hover_shadow'], 'theme')" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : 'Save Effects & Animations' }}
            </button>
            </div>
          </div>

          <!-- Header & Navigation Section -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('header')">
              <h3 class="section-header">🧭 Header & Navigation</h3>
              <span class="collapse-icon">{{ expandedSections['header'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['header']" class="collapsible-content">
            <div class="subsection">
              <h4 class="subsection-title">Header Style</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Header Background</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_header_bg').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_header_bg').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#ffffff"
                    />
                  </div>
                  <p class="form-hint">Header/navbar background color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Header Text Color</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_header_text').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_header_text').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#111827"
                    />
                  </div>
                  <p class="form-hint">Navigation link text color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Header Height (px)</label>
                  <input 
                    v-model="getSetting('theme_header_height').value" 
                    type="number" 
                    class="form-control"
                    placeholder="72"
                    min="50"
                    max="120"
                  />
                  <p class="form-hint">Height of navigation bar (50-120px)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Header Sticky</label>
                  <select v-model="getSetting('theme_header_sticky').value" class="form-control">
                    <option value="true">Enabled (Stays on scroll)</option>
                    <option value="false">Disabled (Scrolls away)</option>
                  </select>
                  <p class="form-hint">Keep header visible when scrolling</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Header Shadow</label>
                  <select v-model="getSetting('theme_header_shadow').value" class="form-control">
                    <option value="none">None</option>
                    <option value="0 1px 3px 0 rgb(0 0 0 / 0.1)">Subtle</option>
                    <option value="0 4px 6px -1px rgb(0 0 0 / 0.1)">Medium</option>
                    <option value="0 10px 15px -3px rgb(0 0 0 / 0.1)">Strong</option>
                  </select>
                  <p class="form-hint">Drop shadow below header</p>
                </div>
              </div>
            </div>

            <button @click="saveSection(['theme_header_bg', 'theme_header_text', 'theme_header_height', 'theme_header_sticky', 'theme_header_shadow'], 'theme')" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : 'Save Header Settings' }}
            </button>
            </div>
          </div>

          <!-- Footer Section -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('footer')">
              <h3 class="section-header">🦶 Footer Styling</h3>
              <span class="collapse-icon">{{ expandedSections['footer'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['footer']" class="collapsible-content">
            <div class="subsection">
              <h4 class="subsection-title">Footer Style</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Footer Background</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_footer_bg').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_footer_bg').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#1f2937"
                    />
                  </div>
                  <p class="form-hint">Footer background color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Footer Text Color</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_footer_text').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_footer_text').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#9ca3af"
                    />
                  </div>
                  <p class="form-hint">Footer text color</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Footer Padding (rem)</label>
                  <input 
                    v-model="getSetting('theme_footer_padding').value" 
                    type="number" 
                    step="0.5"
                    class="form-control"
                    placeholder="3"
                    min="1"
                    max="8"
                  />
                  <p class="form-hint">Vertical padding for footer (1-8 rem)</p>
                </div>
              </div>
            </div>

            <button @click="saveSection(['theme_footer_bg', 'theme_footer_text', 'theme_footer_padding'], 'theme')" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : 'Save Footer Settings' }}
            </button>
            </div>
          </div>

          <!-- Advanced Theme Options -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('advanced')">
              <h3 class="section-header">🚀 Advanced Features</h3>
              <span class="collapse-icon">{{ expandedSections['advanced'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['advanced']" class="collapsible-content">
            <div class="subsection">
              <h4 class="subsection-title">User Experience Enhancements</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label toggle-label">
                    <input
                      type="checkbox"
                      :checked="getSetting('theme_dark_mode_enabled').value === 'true'"
                      @change="handleToggleChange('theme_dark_mode_enabled', $event)"
                      class="toggle-input"
                    />
                    <span class="toggle-slider"></span>
                    Dark Mode Toggle
                  </label>
                  <p class="form-hint">🌙 Let users switch between light and dark themes (adds toggle button to site)</p>
                </div>

                <div class="form-group">
                  <label class="form-label toggle-label">
                    <input
                      type="checkbox"
                      :checked="getSetting('theme_smooth_scroll').value === 'true'"
                      @change="handleToggleChange('theme_smooth_scroll', $event)"
                      class="toggle-input"
                    />
                    <span class="toggle-slider"></span>
                    Smooth Scrolling
                  </label>
                  <p class="form-hint">🎯 Animated scrolling when clicking anchor links (already default in modern browsers)</p>
                </div>

                <div class="form-group">
                  <label class="form-label toggle-label">
                    <input
                      type="checkbox"
                      :checked="getSetting('theme_parallax_enabled').value === 'true'"
                      @change="handleToggleChange('theme_parallax_enabled', $event)"
                      class="toggle-input"
                    />
                    <span class="toggle-slider"></span>
                    Parallax Scrolling
                  </label>
                  <p class="form-hint">🏔️ Background images move slower than foreground (creates 3D depth effect on hero sections)</p>
                </div>

                <div class="form-group">
                  <label class="form-label toggle-label">
                    <input
                      type="checkbox"
                      :checked="getSetting('theme_glass_morphism').value === 'true'"
                      @change="handleToggleChange('theme_glass_morphism', $event)"
                      class="toggle-input"
                    />
                    <span class="toggle-slider"></span>
                    Glass Morphism
                  </label>
                  <p class="form-hint">✨ Frosted glass effect with backdrop blur on cards and modals</p>
                </div>

                <div class="form-group">
                  <label class="form-label toggle-label">
                    <input
                      type="checkbox"
                      :checked="getSetting('theme_animations_enabled').value === 'true'"
                      @change="handleToggleChange('theme_animations_enabled', $event)"
                      class="toggle-input"
                    />
                    <span class="toggle-slider"></span>
                    Page Animations
                  </label>
                  <p class="form-hint">🎬 Elements fade/slide in as they enter viewport (scroll animations)</p>
                </div>

                <div class="form-group">
                  <label class="form-label toggle-label">
                    <input
                      type="checkbox"
                      :checked="getSetting('theme_gradient_overlays').value === 'true'"
                      @change="handleToggleChange('theme_gradient_overlays', $event)"
                      class="toggle-input"
                    />
                    <span class="toggle-slider"></span>
                    Gradient Overlays
                  </label>
                  <p class="form-hint">🌈 Colored gradient overlays on hero images and feature cards</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Custom CSS</label>
                  <textarea 
                    v-model="getSetting('theme_custom_css').value" 
                    class="form-control code-input"
                    rows="10"
                    placeholder="/* Add your custom CSS here */&#10;&#10;.my-custom-class {&#10;  color: red;&#10;  font-weight: bold;&#10;}&#10;&#10;/* Override any site styles */&#10;body {&#10;  /* Your overrides here */&#10;}"
                  ></textarea>
                  <p class="form-hint">💻 Advanced: Inject custom CSS that will be applied site-wide (use with caution)</p>
                </div>
              </div>
            </div>

            <button @click="saveSection(['theme_dark_mode_enabled', 'theme_smooth_scroll', 'theme_parallax_enabled', 'theme_glass_morphism', 'theme_animations_enabled', 'theme_gradient_overlays', 'theme_custom_css'], 'theme')" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : 'Save Advanced Features' }}
            </button>
            </div>
          </div>

          <!-- Visual Effects Section -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('visual-effects')">
              <h3 class="section-header">✨ Visual Effects & Style Presets</h3>
              <span class="collapse-icon">{{ expandedSections['visual-effects'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['visual-effects']" class="collapsible-content">
            <div class="subsection">
              <h4 class="subsection-title">Button Style Preset</h4>
              <div class="button-preset-grid">
                <label class="preset-option">
                  <input type="radio" v-model="getSetting('theme_button_style').value" value="solid" name="button-style" />
                  <div class="preset-demo">
                    <div class="demo-button solid-style">Solid</div>
                  </div>
                  <span class="preset-label">Solid</span>
                </label>
                <label class="preset-option">
                  <input type="radio" v-model="getSetting('theme_button_style').value" value="outlined" name="button-style" />
                  <div class="preset-demo">
                    <div class="demo-button outlined-style">Outlined</div>
                  </div>
                  <span class="preset-label">Outlined</span>
                </label>
                <label class="preset-option">
                  <input type="radio" v-model="getSetting('theme_button_style').value" value="raised" name="button-style" />
                  <div class="preset-demo">
                    <div class="demo-button raised-style">Raised</div>
                  </div>
                  <span class="preset-label">3D Raised</span>
                </label>
                <label class="preset-option">
                  <input type="radio" v-model="getSetting('theme_button_style').value" value="ghost" name="button-style" />
                  <div class="preset-demo">
                    <div class="demo-button ghost-style">Ghost</div>
                  </div>
                  <span class="preset-label">Ghost</span>
                </label>
              </div>
            </div>

            <div class="subsection">
              <h4 class="subsection-title">Corner Style Preset</h4>
              <div class="button-preset-grid">
                <label class="preset-option">
                  <input type="radio" v-model="getSetting('theme_corner_style').value" value="sharp" name="corner-style" />
                  <div class="preset-demo">
                    <div class="demo-card sharp-corners">Sharp</div>
                  </div>
                  <span class="preset-label">Sharp</span>
                </label>
                <label class="preset-option">
                  <input type="radio" v-model="getSetting('theme_corner_style').value" value="rounded" name="corner-style" />
                  <div class="preset-demo">
                    <div class="demo-card rounded-corners">Rounded</div>
                  </div>
                  <span class="preset-label">Rounded</span>
                </label>
                <label class="preset-option">
                  <input type="radio" v-model="getSetting('theme_corner_style').value" value="extra-rounded" name="corner-style" />
                  <div class="preset-demo">
                    <div class="demo-card extra-rounded-corners">Extra</div>
                  </div>
                  <span class="preset-label">Extra Round</span>
                </label>
                <label class="preset-option">
                  <input type="radio" v-model="getSetting('theme_corner_style').value" value="pill" name="corner-style" />
                  <div class="preset-demo">
                    <div class="demo-card pill-corners">Pill</div>
                  </div>
                  <span class="preset-label">Pill</span>
                </label>
              </div>
            </div>

            <div class="subsection">
              <h4 class="subsection-title">Image Effects</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Image Hover Effect</label>
                  <select v-model="getSetting('theme_image_hover').value" class="form-control">
                    <option value="none">None</option>
                    <option value="zoom">Zoom In</option>
                    <option value="zoom-out">Zoom Out</option>
                    <option value="brightness">Brighten</option>
                    <option value="grayscale">Grayscale to Color</option>
                    <option value="blur">Sharpen (Remove Blur)</option>
                  </select>
                  <p class="form-hint">Effect applied to product/category images on hover</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Image Border Style</label>
                  <select v-model="getSetting('theme_image_border_style').value" class="form-control">
                    <option value="none">None</option>
                    <option value="solid">Solid Border</option>
                    <option value="shadow">Drop Shadow</option>
                    <option value="glow">Glow Effect</option>
                    <option value="frame">Picture Frame</option>
                  </select>
                  <p class="form-hint">Border/frame style for images</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Background Pattern</label>
                  <select v-model="getSetting('theme_bg_pattern').value" class="form-control">
                    <option value="none">None (Solid)</option>
                    <option value="dots">Dots</option>
                    <option value="grid">Grid</option>
                    <option value="diagonal">Diagonal Lines</option>
                    <option value="circuit">Circuit</option>
                    <option value="topography">Topography</option>
                  </select>
                  <p class="form-hint">Subtle pattern overlay on page background</p>
                </div>
              </div>
            </div>

            <div class="subsection">
              <h4 class="subsection-title">Text & Shadow Effects</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Heading Text Shadow</label>
                  <select v-model="getSetting('theme_heading_shadow').value" class="form-control">
                    <option value="none">None</option>
                    <option value="subtle">Subtle</option>
                    <option value="strong">Strong</option>
                    <option value="glow">Glow Effect</option>
                    <option value="long">Long Shadow</option>
                  </select>
                  <p class="form-hint">Shadow effect on heading text</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Letter Spacing</label>
                  <select v-model="getSetting('theme_letter_spacing').value" class="form-control">
                    <option value="tight">Tight (-0.025em)</option>
                    <option value="normal">Normal (0)</option>
                    <option value="wide">Wide (0.025em)</option>
                    <option value="wider">Wider (0.05em)</option>
                    <option value="widest">Widest (0.1em)</option>
                  </select>
                  <p class="form-hint">Spacing between letters in headings</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Loading Animation</label>
                  <select v-model="getSetting('theme_loading_animation').value" class="form-control">
                    <option value="spinner">Spinner</option>
                    <option value="dots">Bouncing Dots</option>
                    <option value="pulse">Pulse</option>
                    <option value="bars">Bars</option>
                    <option value="skeleton">Skeleton Screen</option>
                  </select>
                  <p class="form-hint">Loading indicator style</p>
                </div>
              </div>
            </div>

            <button @click="saveSection(['theme_button_style', 'theme_corner_style', 'theme_image_hover', 'theme_image_border_style', 'theme_bg_pattern', 'theme_heading_shadow', 'theme_letter_spacing', 'theme_loading_animation'], 'theme')" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : 'Save Visual Effects' }}
            </button>
            </div>
          </div>

          <!-- Gradient Builder Section -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('gradients')">
              <h3 class="section-header">🌈 Gradient & Background Effects</h3>
              <span class="collapse-icon">{{ expandedSections['gradients'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['gradients']" class="collapsible-content">
            <div class="subsection">
              <h4 class="subsection-title">Hero Background Gradient</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Gradient Color 1</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_gradient_start').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_gradient_start').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#6366f1"
                    />
                  </div>
                  <p class="form-hint">Starting color for gradients</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Gradient Color 2</label>
                  <div class="color-input-group">
                    <input 
                      v-model="getSetting('theme_gradient_end').value" 
                      type="color" 
                      class="color-picker"
                    />
                    <input 
                      v-model="getSetting('theme_gradient_end').value" 
                      type="text" 
                      class="form-control"
                      placeholder="#ec4899"
                    />
                  </div>
                  <p class="form-hint">Ending color for gradients</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Gradient Direction</label>
                  <select v-model="getSetting('theme_gradient_direction').value" class="form-control">
                    <option value="to right">Left to Right →</option>
                    <option value="to left">Right to Left ←</option>
                    <option value="to bottom">Top to Bottom ↓</option>
                    <option value="to top">Bottom to Top ↑</option>
                    <option value="to bottom right">Diagonal ↘</option>
                    <option value="135deg">Diagonal ↖</option>
                  </select>
                  <p class="form-hint">Direction of gradient flow</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Gradient Overlay Opacity</label>
                  <input 
                    v-model="getSetting('theme_gradient_opacity').value" 
                    type="range" 
                    min="0" 
                    max="100" 
                    class="range-slider"
                  />
                  <div class="range-value">{{ getSetting('theme_gradient_opacity').value }}%</div>
                  <p class="form-hint">Transparency of gradient overlay (0-100%)</p>
                </div>
              </div>

              <!-- Gradient Preview -->
              <div class="gradient-preview" :style="{ 
                backgroundImage: `linear-gradient(${getSetting('theme_gradient_direction').value}, ${getSetting('theme_gradient_start').value}, ${getSetting('theme_gradient_end').value})`,
                opacity: parseInt(String(getSetting('theme_gradient_opacity').value)) / 100
              }">
                <span>Gradient Preview</span>
              </div>
            </div>

            <div class="subsection">
              <h4 class="subsection-title">Backdrop & Blur Effects</h4>
              <div class="settings-grid">
                <div class="form-group">
                  <label class="form-label">Backdrop Blur Intensity</label>
                  <input 
                    v-model="getSetting('theme_backdrop_blur').value" 
                    type="range" 
                    min="0" 
                    max="20" 
                    class="range-slider"
                  />
                  <div class="range-value">{{ getSetting('theme_backdrop_blur').value }}px</div>
                  <p class="form-hint">Blur intensity for glass morphism effects (0-20px)</p>
                </div>

                <div class="form-group">
                  <label class="form-label">Modal Overlay Opacity</label>
                  <input 
                    v-model="getSetting('theme_modal_backdrop_opacity').value" 
                    type="range" 
                    min="0" 
                    max="100" 
                    class="range-slider"
                  />
                  <div class="range-value">{{ getSetting('theme_modal_backdrop_opacity').value }}%</div>
                  <p class="form-hint">Darkness of modal background overlay (0-100%)</p>
                </div>
              </div>
            </div>

            <button @click="saveSection(['theme_gradient_start', 'theme_gradient_end', 'theme_gradient_direction', 'theme_gradient_opacity', 'theme_backdrop_blur', 'theme_modal_backdrop_opacity'], 'theme')" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Saving...' : 'Save Gradient Effects' }}
            </button>
            </div>
          </div>

          <!-- Reset Theme -->
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('theme-management')">
              <h3 class="section-header">⚠️ Theme Management</h3>
              <span class="collapse-icon">{{ expandedSections['theme-management'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['theme-management']" class="collapsible-content">
            <div class="alert alert-warning">
              <strong>Reset Theme:</strong> This will restore all theme settings to their default values. This action cannot be undone.
            </div>
            <button 
              @click="confirmResetTheme" 
              class="btn btn-danger"
              :disabled="isActionLoading('resetTheme')"
            >
              <span v-if="isActionLoading('resetTheme')" class="btn-spinner"></span>
              {{ isActionLoading('resetTheme') ? 'Resetting...' : 'Reset All Theme Settings' }}
            </button>
            </div>
          </div>
        </div>

        <!-- Social Media Tab -->
        <div v-if="activeTab === 'social'" class="tab-content">
          <div class="settings-card">
            <div class="section-header-collapsible" @click="toggleSection('social-media')">
              <h3 class="section-header">🔗 Social Media Links</h3>
              <span class="collapse-icon">{{ expandedSections['social-media'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['social-media']" class="collapsible-content">
            <div class="settings-grid">
              <div class="form-group">
                <label class="form-label">Facebook URL</label>
                <input 
                  v-model="getSetting('facebook_url').value" 
                  type="url" 
                  class="form-control"
                  placeholder="https://facebook.com/yourpage"
                />
              </div>

              <div class="form-group">
                <label class="form-label">Instagram URL</label>
                <input 
                  v-model="getSetting('instagram_url').value" 
                  type="url" 
                  class="form-control"
                  placeholder="https://instagram.com/yourpage"
                />
              </div>

              <div class="form-group">
                <label class="form-label">Twitter URL</label>
                <input 
                  v-model="getSetting('twitter_url').value" 
                  type="url" 
                  class="form-control"
                  placeholder="https://twitter.com/yourpage"
                />
              </div>

              <div class="form-group">
                <label class="form-label">YouTube URL</label>
                <input 
                  v-model="getSetting('youtube_url').value" 
                  type="url" 
                  class="form-control"
                  placeholder="https://youtube.com/@yourchannel"
                />
              </div>
            </div>

            <!-- Save Button for Social Media -->
            <div class="form-actions">
              <button @click="saveSection(['facebook_url', 'instagram_url', 'twitter_url', 'youtube_url'], 'social')" class="btn btn-primary btn-lg" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Social Media Settings' }}
              </button>
            </div>
            </div>
          </div>
        </div>

        <!-- Content Tab -->
        <div v-if="activeTab === 'content'" class="tab-content">
          <!-- Homepage Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('homepage')">
              <h3 class="section-header">🏠 Homepage</h3>
              <span class="collapse-icon">{{ expandedSections['homepage'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['homepage']" class="collapsible-content">
            <!-- Hero Section Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">🎭 Hero Section</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Hero Title</label>
                  <input 
                    v-model="getSetting('hero_title').value" 
                    type="text" 
                    class="form-control"
                    placeholder="Premium Powersports Vehicles & Gear"
                  />
                  <p class="form-hint">Main headline on the homepage</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Hero Subtitle</label>
                  <textarea 
                    v-model="getSetting('hero_subtitle').value" 
                    class="form-control"
                    rows="3"
                    placeholder="Discover our collection of ATVs, dirt bikes, UTVs, snowmobiles, and premium gear for all your outdoor adventures."
                  ></textarea>
                  <p class="form-hint">Supporting text below the main headline</p>
                </div>
              </div>
            </div>

            <!-- Features Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">⭐ Why Choose Us Features</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <FeatureListManager 
                    v-model="getSetting('home_features').value"
                    title="Features List"
                  />
                </div>
              </div>
            </div>

            <!-- Brands Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">🏭 Partner Brands</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Brands Section Title</label>
                  <input 
                    v-model="getSetting('brands_section_title').value" 
                    type="text" 
                    class="form-control"
                    placeholder="Brands We Carry"
                  />
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Brands Section Subtitle</label>
                  <input 
                    v-model="getSetting('brands_section_subtitle').value" 
                    type="text" 
                    class="form-control"
                    placeholder="We partner with industry-leading manufacturers"
                  />
                </div>

                <div class="form-group full-width">
                  <BrandListManager 
                    v-model="getSetting('partner_brands').value"
                    title="Brands List"
                  />
                  <p class="form-hint">Add your partner brands with logos. Brands will appear on the homepage and footer.</p>
                </div>
              </div>
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['hero_title', 'hero_subtitle', 'home_features', 'partner_brands', 'brands_section_title', 'brands_section_subtitle'], 'homepage')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Homepage' }}
              </button>
            </div>
            </div>
          </div>

          <!-- Products Page Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('products-page')">
              <h3 class="section-header">📦 Products Page</h3>
              <span class="collapse-icon">{{ expandedSections['products-page'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['products-page']" class="collapsible-content">
            <!-- Page Header Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📄 Page Header</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Page Title</label>
                  <input 
                    v-model="getSetting('products_title').value" 
                    type="text" 
                    class="form-control"
                    placeholder="Our Products"
                  />
                  <p class="form-hint">Title at the top of the products page</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Page Subtitle</label>
                  <textarea 
                    v-model="getSetting('products_subtitle').value" 
                    class="form-control"
                    rows="2"
                    placeholder="Explore our complete collection of powersports vehicles and gear"
                  ></textarea>
                  <p class="form-hint">Description below the page title</p>
                </div>
              </div>
            </div>

            <!-- Success/Error Messages -->

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['products_title', 'products_subtitle'], 'products')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Products Page' }}
              </button>
            </div>
            </div>
          </div>

          <!-- About Page Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('about-page')">
              <h3 class="section-header">ℹ️ About Page</h3>
              <span class="collapse-icon">{{ expandedSections['about-page'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['about-page']" class="collapsible-content">
            <!-- Page Header Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📄 Page Header</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Page Title</label>
                  <input 
                    v-model="getSetting('about_title').value" 
                    type="text" 
                    class="form-control"
                    placeholder="About Us"
                  />
                  <p class="form-hint">Title at the top of the about page</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Page Subtitle</label>
                  <textarea 
                    v-model="getSetting('about_subtitle').value" 
                    class="form-control"
                    rows="2"
                    placeholder="Your trusted partner for powersports adventures"
                  ></textarea>
                  <p class="form-hint">Description below the page title</p>
                </div>
              </div>
            </div>

            <!-- Our Story Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📖 Our Story</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Paragraph 1</label>
                  <textarea 
                    v-model="getSetting('about_story_paragraph1').value" 
                    class="form-control"
                    rows="3"
                    placeholder="Founded with a passion for adventure..."
                  ></textarea>
                  <p class="form-hint">First paragraph of the Our Story section</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Paragraph 2</label>
                  <textarea 
                    v-model="getSetting('about_story_paragraph2').value" 
                    class="form-control"
                    rows="3"
                    placeholder="Our journey began..."
                  ></textarea>
                  <p class="form-hint">Second paragraph of the Our Story section</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Story Image</label>
                  <div v-if="getSetting('about_story_image').value" class="current-setting-image">
                    <img :src="getMediaUrl(getSetting('about_story_image').value)" alt="Story image" />
                    <button @click="getSetting('about_story_image').value = ''" type="button" class="btn btn-sm btn-danger">Remove</button>
                  </div>
                  <button @click="openMediaPickerFor('about_story_image')" type="button" class="btn btn-secondary">
                    {{ getSetting('about_story_image').value ? 'Change Image' : 'Select from Library' }}
                  </button>
                  <p class="form-hint">Select an image for the Our Story section</p>
                </div>
              </div>
            </div>

            <!-- Our Mission Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">🎯 Our Mission</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Mission Statement</label>
                  <textarea 
                    v-model="getSetting('about_mission_text').value" 
                    class="form-control"
                    rows="3"
                    placeholder="To empower outdoor enthusiasts..."
                  ></textarea>
                  <p class="form-hint">Main mission statement text</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Mission Image</label>
                  <div v-if="getSetting('about_mission_image').value" class="current-setting-image">
                    <img :src="getMediaUrl(getSetting('about_mission_image').value)" alt="Mission image" />
                    <button @click="getSetting('about_mission_image').value = ''" type="button" class="btn btn-sm btn-danger">Remove</button>
                  </div>
                  <button @click="openMediaPickerFor('about_mission_image')" type="button" class="btn btn-secondary">
                    {{ getSetting('about_mission_image').value ? 'Change Image' : 'Select from Library' }}
                  </button>
                  <p class="form-hint">Select an image for the Our Mission section</p>
                </div>

                <div class="form-group full-width">
                  <FeatureListManager 
                    v-model="getSetting('about_mission_points').value"
                    title="Mission Key Points"
                  />
                </div>
              </div>
            </div>

            <!-- Our Values Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">⭐ Our Values</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <FeatureListManager 
                    v-model="getSetting('about_values').value"
                    title="Company Values"
                  />
                </div>
              </div>
            </div>

            <!-- Team Members Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">👥 Team Members</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <TeamMemberManager 
                    v-model="getSetting('about_team_members').value"
                  />
                </div>
              </div>
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['about_title', 'about_subtitle', 'about_story_paragraph1', 'about_story_paragraph2', 'about_story_image', 'about_mission_image', 'about_mission_text', 'about_mission_points', 'about_values', 'about_team_members'], 'about')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save About Page' }}
              </button>
            </div>
            </div>
          </div>

          <!-- Contact Page Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('contact-page')">
              <h3 class="section-header">📞 Contact Page</h3>
              <span class="collapse-icon">{{ expandedSections['contact-page'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['contact-page']" class="collapsible-content">
            <!-- Page Header Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📄 Page Header</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Page Title</label>
                  <input 
                    v-model="getSetting('contact_title').value" 
                    type="text" 
                    class="form-control"
                    placeholder="Contact Us"
                  />
                  <p class="form-hint">Title at the top of the contact page</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Page Subtitle</label>
                  <textarea 
                    v-model="getSetting('contact_subtitle').value" 
                    class="form-control"
                    rows="2"
                    placeholder="Get in touch with our team for questions, support, or to schedule a visit"
                  ></textarea>
                  <p class="form-hint">Description below the page title</p>
                </div>
              </div>
            </div>

            <!-- Why Contact Us Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">💡 Why Contact Us</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <FeatureListManager 
                    v-model="getSetting('contact_reasons').value"
                    title="Reasons to Contact"
                    :show-icon="false"
                    :show-description="false"
                  />
                  <p class="form-hint">Reasons why customers should contact you (bullet points)</p>
                </div>
              </div>
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['contact_title', 'contact_subtitle', 'contact_reasons'], 'contact')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Contact Page' }}
              </button>
            </div>
            </div>
          </div>
        </div>

        <!-- Pages Tab -->
        <div v-if="activeTab === 'pages'" class="tab-content">
          <!-- FAQ Page Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('faq-page')">
              <h3 class="section-header">❓ FAQ Page</h3>
              <span class="collapse-icon">{{ expandedSections['faq-page'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['faq-page']" class="collapsible-content">
            <!-- Page Header Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📄 Page Header</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Page Title</label>
                  <input 
                    v-model="getSetting('faq_title').value" 
                    type="text" 
                    class="form-control"
                    placeholder="FAQ"
                  />
                  <p class="form-hint">Title at the top of the FAQ page</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Page Subtitle</label>
                  <textarea 
                    v-model="getSetting('faq_subtitle').value" 
                    class="form-control"
                    rows="2"
                    placeholder="Frequently Asked Questions"
                  ></textarea>
                  <p class="form-hint">Description below the page title</p>
                </div>
              </div>
            </div>

            <!-- Content Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📝 Page Content</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Content (HTML)</label>
                  <textarea 
                    v-model="getSetting('faq_content').value" 
                    class="form-control html-editor"
                    rows="10"
                    placeholder="<h2>General Questions</h2>..."
                  ></textarea>
                  <p class="form-hint">
                    HTML content. Use: &lt;h2&gt; for headings, &lt;h3&gt; for subheadings, &lt;p&gt; for paragraphs, 
                    &lt;ul&gt; and &lt;li&gt; for lists, &lt;strong&gt; for bold, &lt;a href=""&gt; for links
                  </p>
                </div>
              </div>
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['faq_title', 'faq_subtitle', 'faq_content'], 'faq')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save FAQ Page' }}
              </button>
            </div>
            </div>
          </div>

          <!-- Shipping & Returns Page Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('shipping-page')">
              <h3 class="section-header">📦 Shipping & Returns Page</h3>
              <span class="collapse-icon">{{ expandedSections['shipping-page'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['shipping-page']" class="collapsible-content">
            <!-- Page Header Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📄 Page Header</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Page Title</label>
                  <input 
                    v-model="getSetting('shipping_title').value" 
                    type="text" 
                    class="form-control"
                    placeholder="Shipping & Returns"
                  />
                  <p class="form-hint">Title at the top of the shipping page</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Page Subtitle</label>
                  <textarea 
                    v-model="getSetting('shipping_subtitle').value" 
                    class="form-control"
                    rows="2"
                    placeholder="Our shipping and return policies"
                  ></textarea>
                  <p class="form-hint">Description below the page title</p>
                </div>
              </div>
            </div>

            <!-- Content Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📝 Page Content</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Content (HTML)</label>
                  <textarea 
                    v-model="getSetting('shipping_content').value" 
                    class="form-control html-editor"
                    rows="10"
                    placeholder="<h2>Shipping Policy</h2>..."
                  ></textarea>
                  <p class="form-hint">
                    HTML content. Use: &lt;h2&gt; for headings, &lt;h3&gt; for subheadings, &lt;p&gt; for paragraphs, 
                    &lt;ul&gt; and &lt;li&gt; for lists, &lt;strong&gt; for bold, &lt;a href=""&gt; for links
                  </p>
                </div>
              </div>
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['shipping_title', 'shipping_subtitle', 'shipping_content'], 'shipping')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Shipping & Returns Page' }}
              </button>
            </div>
            </div>
          </div>

          <!-- Privacy Policy Page Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('privacy-page')">
              <h3 class="section-header">🔒 Privacy Policy Page</h3>
              <span class="collapse-icon">{{ expandedSections['privacy-page'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['privacy-page']" class="collapsible-content">
            <!-- Page Header Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📄 Page Header</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Page Title</label>
                  <input 
                    v-model="getSetting('privacy_title').value" 
                    type="text" 
                    class="form-control"
                    placeholder="Privacy Policy"
                  />
                  <p class="form-hint">Title at the top of the privacy page</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Page Subtitle</label>
                  <textarea 
                    v-model="getSetting('privacy_subtitle').value" 
                    class="form-control"
                    rows="2"
                    placeholder="How we handle your personal information"
                  ></textarea>
                  <p class="form-hint">Description below the page title</p>
                </div>
              </div>
            </div>

            <!-- Content Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📝 Page Content</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Content (HTML)</label>
                  <textarea 
                    v-model="getSetting('privacy_content').value" 
                    class="form-control html-editor"
                    rows="10"
                    placeholder="<h2>Privacy Policy</h2>..."
                  ></textarea>
                  <p class="form-hint">
                    HTML content. Use: &lt;h2&gt; for headings, &lt;h3&gt; for subheadings, &lt;p&gt; for paragraphs, 
                    &lt;ul&gt; and &lt;li&gt; for lists, &lt;strong&gt; for bold, &lt;a href=""&gt; for links
                  </p>
                </div>
              </div>
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['privacy_title', 'privacy_subtitle', 'privacy_content'], 'privacy')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Privacy Policy Page' }}
              </button>
            </div>
            </div>
          </div>

          <!-- Terms of Service Page Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('terms-page')">
              <h3 class="section-header">📜 Terms of Service Page</h3>
              <span class="collapse-icon">{{ expandedSections['terms-page'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['terms-page']" class="collapsible-content">
            <!-- Page Header Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📄 Page Header</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Page Title</label>
                  <input 
                    v-model="getSetting('terms_title').value" 
                    type="text" 
                    class="form-control"
                    placeholder="Terms of Service"
                  />
                  <p class="form-hint">Title at the top of the terms page</p>
                </div>

                <div class="form-group full-width">
                  <label class="form-label">Page Subtitle</label>
                  <textarea 
                    v-model="getSetting('terms_subtitle').value" 
                    class="form-control"
                    rows="2"
                    placeholder="Terms and conditions for using our services"
                  ></textarea>
                  <p class="form-hint">Description below the page title</p>
                </div>
              </div>
            </div>

            <!-- Page Content Subsection -->
            <div class="subsection">
              <h4 class="subsection-title">📝 Page Content</h4>
              <div class="settings-grid">
                <div class="form-group full-width">
                  <label class="form-label">Page Content (HTML)</label>
                  <textarea 
                    v-model="getSetting('terms_content').value" 
                    class="form-control html-editor"
                    rows="10"
                    placeholder="<h2>Terms of Service</h2>..."
                  ></textarea>
                  <p class="form-hint">
                    HTML content. Use: &lt;h2&gt; for headings, &lt;h3&gt; for subheadings, &lt;p&gt; for paragraphs, 
                    &lt;ul&gt; and &lt;li&gt; for lists, &lt;strong&gt; for bold, &lt;a href=""&gt; for links
                  </p>
                </div>
              </div>
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['terms_title', 'terms_subtitle', 'terms_content'], 'terms')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Terms of Service Page' }}
              </button>
            </div>
            </div>
          </div>
        </div>

        <!-- Advanced Tab -->
        <div v-if="activeTab === 'email'" class="tab-content">
          <!-- SMTP Configuration -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('smtp-settings')">
              <h3 class="section-header">📧 SMTP Configuration</h3>
              <span class="collapse-icon">{{ expandedSections['smtp-settings'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['smtp-settings']" class="collapsible-content">
            <div class="settings-grid">
              <div class="form-group">
                <label class="form-label toggle-label">
                  <input
                    type="checkbox"
                    :checked="getSetting('smtp_enabled').value === 'true'"
                    @change="handleToggleChange('smtp_enabled', $event)"
                    class="toggle-input"
                  />
                  <span class="toggle-slider"></span>
                  Enable Email (SMTP)
                </label>
                <p class="form-hint">Enable outbound email sending via SMTP</p>
              </div>

              <div class="form-group">
                <label class="form-label">SMTP Provider</label>
                <select
                  v-model="smtpHostProvider"
                  @change="handleSmtpProviderChange"
                  class="form-control"
                >
                  <option v-for="provider in smtpProviders" :key="provider.host" :value="provider.host">
                    {{ provider.label }}
                  </option>
                </select>
                <p class="form-hint">Select your email provider</p>
              </div>

              <div v-if="smtpHostProvider === 'custom'" class="form-group">
                <label class="form-label">Custom SMTP Host</label>
                <input
                  v-model="customSmtpHost"
                  @input="handleCustomHostChange"
                  type="text"
                  class="form-control"
                  placeholder="smtp.example.com"
                />
                <p class="form-hint">Enter your custom SMTP server hostname</p>
              </div>

              <div class="form-group">
                <label class="form-label">SMTP Port</label>
                <input
                  v-model="getSetting('smtp_port').value"
                  type="number"
                  class="form-control"
                  placeholder="587"
                  min="1"
                  max="65535"
                />
                <p class="form-hint">SMTP port (typically 587 for TLS, 465 for SSL, 25 for plain)</p>
              </div>

              <div class="form-group">
                <label class="form-label">SMTP Username</label>
                <input
                  v-model="getSetting('smtp_username').value"
                  type="text"
                  class="form-control"
                  placeholder="your@email.com"
                />
                <p class="form-hint">Username for SMTP authentication</p>
              </div>

              <div class="form-group">
                <label class="form-label">SMTP Password</label>
                <input
                  v-model="getSetting('smtp_password').value"
                  type="password"
                  class="form-control"
                  placeholder="App password or SMTP password"
                />
                <p class="form-hint">Password for SMTP authentication (stored securely)</p>
              </div>

              <div class="form-group">
                <label class="form-label">From Email Address</label>
                <input
                  v-model="getSetting('smtp_from_email').value"
                  type="email"
                  class="form-control"
                  placeholder="noreply@yourdomain.com"
                />
                <p class="form-hint">Email address that outgoing emails will be sent from</p>
              </div>

              <div class="form-group">
                <label class="form-label">From Name</label>
                <input
                  v-model="getSetting('smtp_from_name').value"
                  type="text"
                  class="form-control"
                  placeholder="701 Performance Power"
                />
                <p class="form-hint">Display name shown in the sender field</p>
              </div>

              <div class="form-group">
                <label class="form-label toggle-label">
                  <input
                    type="checkbox"
                    :checked="getSetting('smtp_use_ssl').value === 'true'"
                    @change="handleToggleChange('smtp_use_ssl', $event)"
                    class="toggle-input"
                  />
                  <span class="toggle-slider"></span>
                  Use SSL/TLS
                </label>
                <p class="form-hint">Enable SSL/TLS encryption for SMTP connection</p>
              </div>

              <div class="form-group">
                <label class="form-label">Site URL</label>
                <input
                  v-model="getSetting('site_url').value"
                  type="url"
                  class="form-control"
                  placeholder="http://localhost:3000"
                />
                <p class="form-hint">Public URL of this website (used in email links)</p>
              </div>
            </div>

            <div class="form-actions">
              <button @click="saveSection(['smtp_enabled','smtp_host','smtp_port','smtp_username','smtp_password','smtp_from_email','smtp_from_name','smtp_use_ssl','site_url'], 'smtp')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Email Settings' }}
              </button>
            </div>
            </div>
          </div>

          <!-- Test Email -->
          <div class="content-section">
            <h3 class="section-header">🧪 Test Email</h3>
            <div class="settings-grid">
              <div class="form-group">
                <label class="form-label">Send Test Email To</label>
                <input
                  v-model="testEmailAddress"
                  type="email"
                  class="form-control"
                  placeholder="test@example.com"
                />
                <p class="form-hint">Send a test email to verify your SMTP settings are working</p>
              </div>
            </div>

            <div v-if="testEmailSuccess" class="alert alert-success">
              {{ testEmailSuccess }}
            </div>
            <div v-if="testEmailError" class="alert alert-danger">
              {{ testEmailError }}
            </div>

            <div class="form-actions">
              <button 
                @click="sendTestEmail" 
                class="btn btn-secondary" 
                :disabled="isActionLoading('sendTestEmail')"
              >
                <span v-if="isActionLoading('sendTestEmail')" class="btn-spinner"></span>
                <span v-else class="icon">📤</span>
                {{ isActionLoading('sendTestEmail') ? 'Sending...' : 'Send Test Email' }}
              </button>
            </div>
          </div>

          <!-- Email Verification note -->
          <div class="content-section">
            <h3 class="section-header">✅ Email Verification</h3>
            <p class="form-hint" style="font-size: 0.95rem; margin-bottom: 1rem;">
              The <strong>Require Email Verification</strong> toggle is located in the
              <strong>Advanced</strong> tab under Security &amp; Access. 
              <span v-if="getSetting('require_email_verification').value === 'true'" style="color: #27ae60; font-weight: 600;">
                ✓ Currently Enabled
              </span>
              <span v-else style="color: #e74c3c; font-weight: 600;">
                ✗ Currently Disabled
              </span>
              — Enable it in the Advanced tab to require new users to verify their email address before logging in.
            </p>
          </div>
        </div>

        <div v-if="activeTab === 'advanced'" class="tab-content">
          <!-- Security & Access Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('security-settings')">
              <h3 class="section-header">🔒 Security & Access</h3>
              <span class="collapse-icon">{{ expandedSections['security-settings'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['security-settings']" class="collapsible-content">
            <div class="settings-grid">
              <div class="form-group">
                <label class="form-label">Session Timeout (minutes)</label>
                <input 
                  v-model="getSetting('session_timeout').value" 
                  type="number" 
                  class="form-control"
                  placeholder="480"
                  min="5"
                  max="1440"
                />
                <p class="form-hint">How long users stay logged in (5-1440 minutes). Default: 480 minutes (8 hours)</p>
              </div>

              <div class="form-group">
                <label class="form-label">Max Login Attempts</label>
                <input 
                  v-model="getSetting('max_login_attempts').value" 
                  type="number" 
                  class="form-control"
                  placeholder="5"
                  min="1"
                  max="20"
                />
                <p class="form-hint">Maximum failed login attempts before lockout</p>
              </div>

              <div class="form-group">
                <label class="form-label toggle-label">
                  <input 
                    type="checkbox" 
                    :checked="getSetting('allow_user_registration').value === 'true'"
                    @change="handleToggleChange('allow_user_registration', $event)"
                    class="toggle-input"
                  />
                  <span class="toggle-slider"></span>
                  Allow User Registration
                </label>
                <p class="form-hint">Enable public user account creation</p>
              </div>

              <div class="form-group">
                <label class="form-label toggle-label">
                  <input 
                    type="checkbox" 
                    :checked="getSetting('require_email_verification').value === 'true'"
                    @change="handleToggleChange('require_email_verification', $event)"
                    class="toggle-input"
                  />
                  <span class="toggle-slider"></span>
                  Require Email Verification
                </label>
                <p class="form-hint">Users must verify email before accessing account</p>
              </div>

              <div class="form-group">
                <label class="form-label toggle-label">
                  <input 
                    type="checkbox" 
                    :checked="getSetting('enable_two_factor_auth').value === 'true'"
                    @change="handleToggleChange('enable_two_factor_auth', $event)"
                    class="toggle-input"
                  />
                  <span class="toggle-slider"></span>
                  Enable Two-Factor Authentication
                  <span class="coming-soon-badge">Coming Soon</span>
                </label>
                <p class="form-hint">Require 2FA for enhanced security (not yet implemented)</p>
              </div>

              <div class="form-group">
                <label class="form-label toggle-label">
                  <input 
                    type="checkbox" 
                    :checked="getSetting('allow_guest_checkout').value === 'true'"
                    @change="handleToggleChange('allow_guest_checkout', $event)"
                    class="toggle-input"
                  />
                  <span class="toggle-slider"></span>
                  Allow Guest Checkout
                  <span class="coming-soon-badge">Coming Soon</span>
                </label>
                <p class="form-hint">Let users checkout without creating account (not yet implemented)</p>
              </div>
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['session_timeout', 'max_login_attempts', 'allow_user_registration', 'require_email_verification', 'enable_two_factor_auth', 'allow_guest_checkout'], 'security')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Security Settings' }}
              </button>
            </div>
            </div>
          </div>

          <!-- Performance & Caching Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('performance-caching')">
              <h3 class="section-header">⚡ Performance & Caching</h3>
              <span class="collapse-icon">{{ expandedSections['performance-caching'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['performance-caching']" class="collapsible-content">
            <div class="settings-grid">
              <div class="form-group">
                <label class="form-label">Image Quality (%)</label>
                <input 
                  v-model="getSetting('image_quality').value" 
                  type="number" 
                  class="form-control"
                  placeholder="85"
                  min="1"
                  max="100"
                />
                <p class="form-hint">Compression quality for uploaded images (1-100)</p>
              </div>

              <div class="form-group">
                <label class="form-label">Cache Duration (hours)</label>
                <input 
                  v-model="getSetting('cache_duration').value" 
                  type="number" 
                  class="form-control"
                  placeholder="24"
                  min="0"
                  max="720"
                />
                <p class="form-hint">How long to cache content (0-720 hours)</p>
              </div>

              <div class="form-group">
                <label class="form-label">Max Image Width (px)</label>
                <input 
                  v-model="getSetting('max_image_width').value" 
                  type="number" 
                  class="form-control"
                  placeholder="1920"
                  min="100"
                  max="5000"
                />
                <p class="form-hint">Maximum width for uploaded images</p>
              </div>

              <div class="form-group">
                <label class="form-label">Max Image Height (px)</label>
                <input 
                  v-model="getSetting('max_image_height').value" 
                  type="number" 
                  class="form-control"
                  placeholder="1080"
                  min="100"
                  max="5000"
                />
                <p class="form-hint">Maximum height for uploaded images</p>
              </div>

              <div class="form-group">
                <label class="form-label toggle-label">
                  <input 
                    type="checkbox" 
                    :checked="getSetting('enable_image_optimization').value === 'true'"
                    @change="handleToggleChange('enable_image_optimization', $event)"
                    class="toggle-input"
                  />
                  <span class="toggle-slider"></span>
                  Enable Image Optimization
                </label>
                <p class="form-hint">Automatically optimize uploaded images</p>
              </div>

              <div class="form-group">
                <label class="form-label toggle-label">
                  <input 
                    type="checkbox" 
                    :checked="getSetting('enable_cdn').value === 'true'"
                    @change="handleToggleChange('enable_cdn', $event)"
                    class="toggle-input"
                  />
                  <span class="toggle-slider"></span>
                  Enable CDN
                </label>
                <p class="form-hint">Use Content Delivery Network for static assets</p>
              </div>

              <div class="form-group">
                <label class="form-label toggle-label">
                  <input 
                    type="checkbox" 
                    :checked="getSetting('enable_compression').value === 'true'"
                    @change="handleToggleChange('enable_compression', $event)"
                    class="toggle-input"
                  />
                  <span class="toggle-slider"></span>
                  Enable Compression
                </label>
                <p class="form-hint">Compress responses for faster delivery</p>
              </div>
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['image_quality', 'cache_duration', 'max_image_width', 'max_image_height', 'enable_image_optimization', 'enable_cdn', 'enable_compression'], 'performance')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Performance Settings' }}
              </button>
            </div>
            </div> <!-- End collapsible-content -->
          </div>

          <!-- System Settings Section -->
          <div class="content-section">
            <div class="section-header-collapsible" @click="toggleSection('maintenance')">
              <h3 class="section-header">🔧 System Settings</h3>
              <span class="collapse-icon">{{ expandedSections['maintenance'] ? '▼' : '▶' }}</span>
            </div>
            
            <div v-show="expandedSections['maintenance']" class="collapsible-content">
            <div class="settings-grid">
              <div class="form-group">
                <label class="form-label toggle-label">
                  <input 
                    type="checkbox" 
                    :checked="getSetting('enable_maintenance_mode').value === 'true'"
                    @change="handleToggleChange('enable_maintenance_mode', $event)"
                    class="toggle-input"
                  />
                  <span class="toggle-slider"></span>
                  Enable Maintenance Mode
                </label>
                <p class="form-hint">Put site in maintenance mode for updates</p>
              </div>
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['enable_maintenance_mode'], 'system')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save System Settings' }}
              </button>
            </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Media Picker -->
      <MediaPicker 
        :is-open="showMediaPicker"
        @close="showMediaPicker = false"
        @select="handleMediaSelection"
        media-type="Image"
      />
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import FeatureListManager from '@/components/FeatureListManager.vue'
import BrandListManager from '@/components/BrandListManager.vue'
import TeamMemberManager from '@/components/TeamMemberManager.vue'
import MediaPicker from '@/components/MediaPicker.vue'
import { useAuthStore } from '@/stores/auth'
import { logDebug, logError } from '@/services/logger'
import { useToast } from '@/composables/useToast'
import { useLoadingState } from '@/composables/useLoadingState'
import { SUCCESS_MESSAGE_DURATION_MS } from '@/constants'
import { apiClient, apiGet, apiPost, apiPut, apiDelete } from '@/utils/apiClient'
import { API_URL, getMediaUrl } from '@/utils/api-config'

interface SiteSetting {
  id: number
  key: string
  displayName: string
  value: string
  type: string
  category: string
  sortOrder: number
  isRequired: boolean
}

const authStore = useAuthStore()
const toast = useToast()
const { isLoading, executeWithLoading, isActionLoading } = useLoadingState()

const activeTab = ref('general')
const activePresetTab = ref('general')
const settings = ref<SiteSetting[]>([])
const error = ref('')
const testEmailAddress = ref('')
const testEmailSuccess = ref('')
const testEmailError = ref('')

// Computed to check if any save action is in progress
const saving = computed(() => {
  return isActionLoading('save-general') ||
         isActionLoading('save-theme') ||
         isActionLoading('save-social') ||
         isActionLoading('save-homepage') ||
         isActionLoading('save-products') ||
         isActionLoading('save-about') ||
         isActionLoading('save-contact') ||
         isActionLoading('save-faq') ||
         isActionLoading('save-shipping') ||
         isActionLoading('save-privacy') ||
         isActionLoading('save-terms') ||
         isActionLoading('save-smtp') ||
         isActionLoading('preset-Modern & Clean') ||
         isActionLoading('preset-Bold & Dynamic') ||
         isActionLoading('preset-Minimal & Elegant') ||
         isActionLoading('preset-Dark & Mysterious') ||
         isActionLoading('preset-Warm & Inviting') ||
         isActionLoading('preset-Tech & Futuristic')
})

// Collapsible sections state - default some sections to expanded
const expandedSections = ref<Record<string, boolean>>({
  'general-settings': true,
  'theme-presets': true,
  'color-palette': false,
  'typography': false,
  'components': false,
  'layout': false,
  'effects': false,
  'header': false,
  'footer': false,
  'advanced': false,
  'visual-effects': false,
  'gradients': false,
  'theme-management': false,
  'social-media': false,
  'homepage': false,
  'products-page': false,
  'about-page': false,
  'contact-page': false,
  'faq-page': false,
  'shipping-page': false,
  'privacy-page': false,
  'terms-page': false,
  'smtp-settings': false,
  'security-settings': false,
  'performance-caching': false,
  'maintenance': false,
  'auto-backup': false
})

const toggleSection = (sectionId: string) => {
  expandedSections.value[sectionId] = !expandedSections.value[sectionId]
}

// Media Picker state
const showMediaPicker = ref(false)
const mediaPickerTargetSetting = ref<string | null>(null)

// SMTP provider presets
const smtpProviders = [
  { label: 'Gmail', host: 'smtp.gmail.com', port: '587' },
  { label: 'Outlook/Hotmail', host: 'smtp-mail.outlook.com', port: '587' },
  { label: 'Yahoo Mail', host: 'smtp.mail.yahoo.com', port: '587' },
  { label: 'SendGrid', host: 'smtp.sendgrid.net', port: '587' },
  { label: 'Mailgun', host: 'smtp.mailgun.org', port: '587' },
  { label: 'Amazon SES', host: 'email-smtp.us-east-1.amazonaws.com', port: '587' },
  { label: 'Custom', host: 'custom', port: '587' }
]

const smtpHostProvider = ref('custom')
const customSmtpHost = ref('')

// Theme Presets - Complete predefined themes
const themePresets = [
  {
    name: 'Modern Light',
    icon: '☀️',
    category: 'general',
    description: 'Clean and bright design',
    preview: { primary: '#6366f1', secondary: '#ec4899', bg: '#ffffff' },
    settings: {
      theme_primary_color: '#6366f1',
      theme_secondary_color: '#ec4899',
      theme_accent_color: '#f59e0b',
      theme_bg_color: '#ffffff',
      theme_bg_secondary: '#f9fafb',
      theme_bg_muted: '#f3f4f6',
      theme_text_primary: '#111827',
      theme_text_secondary: '#6b7280',
      theme_text_muted: '#9ca3af',
      theme_header_bg: '#ffffff',
      theme_header_text: '#111827',
      theme_footer_bg: '#1f2937',
      theme_footer_text: '#ffffff',
      theme_button_radius: '8',
      theme_card_radius: '12',
      theme_gradient_start: '#6366f1',
      theme_gradient_end: '#ec4899',
      theme_gradient_direction: '135deg',
      theme_gradient_opacity: '80'
    }
  },
  {
    name: 'Dark Pro',
    icon: '🌙',
    category: 'general',
    description: 'Sleek dark theme',
    preview: { primary: '#8b5cf6', secondary: '#ec4899', bg: '#0f172a' },
    settings: {
      theme_primary_color: '#8b5cf6',
      theme_secondary_color: '#ec4899',
      theme_accent_color: '#06b6d4',
      theme_bg_color: '#0f172a',
      theme_bg_secondary: '#1e293b',
      theme_bg_muted: '#334155',
      theme_text_primary: '#f1f5f9',
      theme_text_secondary: '#cbd5e1',
      theme_text_muted: '#94a3b8',
      theme_header_bg: '#0f172a',
      theme_header_text: '#f1f5f9',
      theme_footer_bg: '#020617',
      theme_footer_text: '#f1f5f9',
      theme_button_radius: '6',
      theme_card_radius: '16',
      theme_gradient_start: '#8b5cf6',
      theme_gradient_end: '#1e293b',
      theme_gradient_direction: '135deg',
      theme_gradient_opacity: '90'
    }
  },
  {
    name: 'Ocean Blue',
    icon: '🌊',
    category: 'general',
    description: 'Calm ocean vibes',
    preview: { primary: '#0ea5e9', secondary: '#06b6d4', bg: '#f0f9ff' },
    settings: {
      theme_primary_color: '#0ea5e9',
      theme_secondary_color: '#06b6d4',
      theme_accent_color: '#14b8a6',
      theme_bg_color: '#f0f9ff',
      theme_bg_secondary: '#e0f2fe',
      theme_bg_muted: '#bae6fd',
      theme_text_primary: '#0c4a6e',
      theme_text_secondary: '#075985',
      theme_text_muted: '#0369a1',
      theme_header_bg: '#0ea5e9',
      theme_header_text: '#ffffff',
      theme_footer_bg: '#075985',
      theme_footer_text: '#ffffff',
      theme_button_radius: '20',
      theme_card_radius: '16',
      theme_gradient_start: '#0ea5e9',
      theme_gradient_end: '#06b6d4',
      theme_gradient_direction: 'to right',
      theme_gradient_opacity: '85'
    }
  },
  {
    name: 'Sunset',
    icon: '🌅',
    category: 'general',
    description: 'Warm and vibrant',
    preview: { primary: '#f97316', secondary: '#ef4444', bg: '#fff7ed' },
    settings: {
      theme_primary_color: '#f97316',
      theme_secondary_color: '#ef4444',
      theme_accent_color: '#fbbf24',
      theme_bg_color: '#fff7ed',
      theme_bg_secondary: '#ffedd5',
      theme_bg_muted: '#fed7aa',
      theme_text_primary: '#7c2d12',
      theme_text_secondary: '#9a3412',
      theme_text_muted: '#c2410c',
      theme_header_bg: '#ffffff',
      theme_header_text: '#7c2d12',
      theme_footer_bg: '#7c2d12',
      theme_footer_text: '#ffffff',
      theme_button_radius: '12',
      theme_card_radius: '20',
      theme_gradient_start: '#f97316',
      theme_gradient_end: '#ef4444',
      theme_gradient_direction: '135deg',
      theme_gradient_opacity: '75'
    }
  },
  {
    name: 'Forest',
    icon: '🌲',
    category: 'general',
    description: 'Natural earth tones',
    preview: { primary: '#16a34a', secondary: '#84cc16', bg: '#f0fdf4' },
    settings: {
      theme_primary_color: '#16a34a',
      theme_secondary_color: '#84cc16',
      theme_accent_color: '#22c55e',
      theme_bg_color: '#f0fdf4',
      theme_bg_secondary: '#dcfce7',
      theme_bg_muted: '#bbf7d0',
      theme_text_primary: '#14532d',
      theme_text_secondary: '#166534',
      theme_text_muted: '#15803d',
      theme_header_bg: '#ffffff',
      theme_header_text: '#14532d',
      theme_footer_bg: '#14532d',
      theme_footer_text: '#ffffff',
      theme_button_radius: '8',
      theme_card_radius: '12',
      theme_gradient_start: '#16a34a',
      theme_gradient_end: '#84cc16',
      theme_gradient_direction: 'to bottom right',
      theme_gradient_opacity: '70'
    }
  },
  {
    name: 'Purple Dream',
    icon: '💜',
    category: 'general',
    description: 'Elegant purple',
    preview: { primary: '#a855f7', secondary: '#d946ef', bg: '#faf5ff' },
    settings: {
      theme_primary_color: '#a855f7',
      theme_secondary_color: '#d946ef',
      theme_accent_color: '#c026d3',
      theme_bg_color: '#faf5ff',
      theme_bg_secondary: '#f3e8ff',
      theme_bg_muted: '#e9d5ff',
      theme_text_primary: '#581c87',
      theme_text_secondary: '#6b21a8',
      theme_text_muted: '#7e22ce',
      theme_header_bg: '#ffffff',
      theme_header_text: '#581c87',
      theme_footer_bg: '#581c87',
      theme_footer_text: '#ffffff',
      theme_button_radius: '16',
      theme_card_radius: '20',
      theme_gradient_start: '#a855f7',
      theme_gradient_end: '#d946ef',
      theme_gradient_direction: '135deg',
      theme_gradient_opacity: '75'
    }
  },
  {
    name: 'Monochrome',
    icon: '⚫',
    category: 'general',
    description: 'Classic black & white',
    preview: { primary: '#000000', secondary: '#404040', bg: '#ffffff' },
    settings: {
      theme_primary_color: '#000000',
      theme_secondary_color: '#404040',
      theme_accent_color: '#737373',
      theme_bg_color: '#ffffff',
      theme_bg_secondary: '#f5f5f5',
      theme_bg_muted: '#e5e5e5',
      theme_text_primary: '#000000',
      theme_text_secondary: '#404040',
      theme_text_muted: '#737373',
      theme_header_bg: '#000000',
      theme_header_text: '#ffffff',
      theme_footer_bg: '#000000',
      theme_footer_text: '#ffffff',
      theme_button_radius: '0',
      theme_card_radius: '0',
      theme_gradient_start: '#000000',
      theme_gradient_end: '#404040',
      theme_gradient_direction: 'to right',
      theme_gradient_opacity: '95'
    }
  },
  {
    name: 'Mint Fresh',
    icon: '🍃',
    category: 'general',
    description: 'Cool mint palette',
    preview: { primary: '#14b8a6', secondary: '#06b6d4', bg: '#f0fdfa' },
    settings: {
      theme_primary_color: '#14b8a6',
      theme_secondary_color: '#06b6d4',
      theme_accent_color: '#10b981',
      theme_bg_color: '#f0fdfa',
      theme_bg_secondary: '#ccfbf1',
      theme_bg_muted: '#99f6e4',
      theme_text_primary: '#134e4a',
      theme_text_secondary: '#115e59',
      theme_text_muted: '#0f766e',
      theme_header_bg: '#ffffff',
      theme_header_text: '#134e4a',
      theme_footer_bg: '#134e4a',
      theme_footer_text: '#ffffff',
      theme_button_radius: '24',
      theme_card_radius: '16',
      theme_gradient_start: '#14b8a6',
      theme_gradient_end: '#06b6d4',
      theme_gradient_direction: 'to bottom right',
      theme_gradient_opacity: '80'
    }
  },
  {
    name: 'KTM Racing',
    icon: '🏍️',
    category: 'motorsports',
    description: 'Bold orange & black',
    preview: { primary: '#ff6b00', secondary: '#000000', bg: '#fff8f0' },
    settings: {
      theme_primary_color: '#ff6b00',
      theme_secondary_color: '#000000',
      theme_accent_color: '#ff8c00',
      theme_bg_color: '#fff8f0',
      theme_bg_secondary: '#ffe8d6',
      theme_bg_muted: '#ffd4b3',
      theme_text_primary: '#1a1a1a',
      theme_text_secondary: '#404040',
      theme_text_muted: '#666666',
      theme_header_bg: '#ff6b00',
      theme_header_text: '#ffffff',
      theme_footer_bg: '#000000',
      theme_footer_text: '#ffffff',
      theme_button_radius: '4',
      theme_card_radius: '8',
      theme_gradient_start: '#ff6b00',
      theme_gradient_end: '#000000',
      theme_gradient_direction: '135deg',
      theme_gradient_opacity: '85'
    }
  },
  {
    name: 'Kawasaki Thunder',
    icon: '🏁',
    category: 'motorsports',
    description: 'Electric lime green',
    preview: { primary: '#00a651', secondary: '#1a1a1a', bg: '#f0fff4' },
    settings: {
      theme_primary_color: '#00a651',
      theme_secondary_color: '#1a1a1a',
      theme_accent_color: '#22c55e',
      theme_bg_color: '#f0fff4',
      theme_bg_secondary: '#dcfce7',
      theme_bg_muted: '#bbf7d0',
      theme_text_primary: '#0a2f1f',
      theme_text_secondary: '#14532d',
      theme_text_muted: '#166534',
      theme_header_bg: '#00a651',
      theme_header_text: '#ffffff',
      theme_footer_bg: '#1a1a1a',
      theme_footer_text: '#ffffff',
      theme_button_radius: '6',
      theme_card_radius: '12',
      theme_gradient_start: '#00a651',
      theme_gradient_end: '#1a1a1a',
      theme_gradient_direction: '135deg',
      theme_gradient_opacity: '90'
    }
  },
  {
    name: 'Yamaha Racing',
    icon: '🔵',
    category: 'motorsports',
    description: 'Classic racing blue',
    preview: { primary: '#0033a0', secondary: '#ffffff', bg: '#eff6ff' },
    settings: {
      theme_primary_color: '#0033a0',
      theme_secondary_color: '#0ea5e9',
      theme_accent_color: '#3b82f6',
      theme_bg_color: '#eff6ff',
      theme_bg_secondary: '#dbeafe',
      theme_bg_muted: '#bfdbfe',
      theme_text_primary: '#0c1d3d',
      theme_text_secondary: '#1e3a8a',
      theme_text_muted: '#1e40af',
      theme_header_bg: '#0033a0',
      theme_header_text: '#ffffff',
      theme_footer_bg: '#1e293b',
      theme_footer_text: '#ffffff',
      theme_button_radius: '8',
      theme_card_radius: '16',
      theme_gradient_start: '#0033a0',
      theme_gradient_end: '#0ea5e9',
      theme_gradient_direction: 'to right',
      theme_gradient_opacity: '85'
    }
  },
  {
    name: 'Honda Power',
    icon: '🔴',
    category: 'motorsports',
    description: 'Bold racing red',
    preview: { primary: '#cc0000', secondary: '#1a1a1a', bg: '#fef2f2' },
    settings: {
      theme_primary_color: '#cc0000',
      theme_secondary_color: '#1a1a1a',
      theme_accent_color: '#ef4444',
      theme_bg_color: '#fef2f2',
      theme_bg_secondary: '#fee2e2',
      theme_bg_muted: '#fecaca',
      theme_text_primary: '#450a0a',
      theme_text_secondary: '#7f1d1d',
      theme_text_muted: '#991b1b',
      theme_header_bg: '#cc0000',
      theme_header_text: '#ffffff',
      theme_footer_bg: '#1a1a1a',
      theme_footer_text: '#ffffff',
      theme_button_radius: '6',
      theme_card_radius: '10',
      theme_gradient_start: '#cc0000',
      theme_gradient_end: '#450a0a',
      theme_gradient_direction: '135deg',
      theme_gradient_opacity: '80'
    }
  },
  {
    name: 'Can-Am Spirit',
    icon: '🟡',
    category: 'motorsports',
    description: 'High-energy yellow',
    preview: { primary: '#ffd100', secondary: '#000000', bg: '#fefce8' },
    settings: {
      theme_primary_color: '#ffd100',
      theme_secondary_color: '#000000',
      theme_accent_color: '#facc15',
      theme_bg_color: '#fefce8',
      theme_bg_secondary: '#fef9c3',
      theme_bg_muted: '#fef08a',
      theme_text_primary: '#1a1a1a',
      theme_text_secondary: '#404040',
      theme_text_muted: '#525252',
      theme_header_bg: '#ffd100',
      theme_header_text: '#000000',
      theme_footer_bg: '#000000',
      theme_footer_text: '#ffffff',
      theme_button_radius: '8',
      theme_card_radius: '12',
      theme_gradient_start: '#ffd100',
      theme_gradient_end: '#000000',
      theme_gradient_direction: '135deg',
      theme_gradient_opacity: '75'
    }
  },
  {
    name: 'Polaris Adventure',
    icon: '🚜',
    category: 'motorsports',
    description: 'Rugged red & steel',
    preview: { primary: '#c8102e', secondary: '#64748b', bg: '#fef2f2' },
    settings: {
      theme_primary_color: '#c8102e',
      theme_secondary_color: '#64748b',
      theme_accent_color: '#dc2626',
      theme_bg_color: '#fef2f2',
      theme_bg_secondary: '#f1f5f9',
      theme_bg_muted: '#e2e8f0',
      theme_text_primary: '#0f172a',
      theme_text_secondary: '#334155',
      theme_text_muted: '#475569',
      theme_header_bg: '#c8102e',
      theme_header_text: '#ffffff',
      theme_footer_bg: '#334155',
      theme_footer_text: '#ffffff',
      theme_button_radius: '6',
      theme_card_radius: '14',
      theme_gradient_start: '#c8102e',
      theme_gradient_end: '#64748b',
      theme_gradient_direction: 'to bottom right',
      theme_gradient_opacity: '85'
    }
  }
]

// Apply a theme preset
const applyThemePreset = async (preset: typeof themePresets[0]) => {
  await executeWithLoading(async () => {
    try {
      // First reload settings to ensure we have the latest state from DB
      await loadSettings()
      
      // Apply each setting from the preset (only those that already exist)
      for (const [key, value] of Object.entries(preset.settings)) {
        const existingSetting = settings.value.find(s => s.key === key)
        if (existingSetting) {
          existingSetting.value = value
        }
      }
      
      // Save the active preset name
      const presetSetting = settings.value.find(s => s.key === 'theme_preset_active')
      if (presetSetting) {
        presetSetting.value = preset.name
      }
      
      // Get all the setting keys to save
      const settingKeys = [...Object.keys(preset.settings), 'theme_preset_active']
      
      // Split into existing settings (PUT) and missing ones (POST to create)
      const settingsToUpdate = settings.value.filter(s => settingKeys.includes(s.key) && s.id > 0)
      const existingKeys = settingsToUpdate.map(s => s.key)
      const missingKeys = settingKeys.filter(k => !existingKeys.includes(k))
      
      const allPromises: Promise<Response>[] = []

      // Update existing settings via PUT
      for (const setting of settingsToUpdate) {
        allPromises.push(
          fetch(`${API_URL}/admin/settings/${setting.id}`, {
            method: 'PUT',
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${authStore.token}`
            },
            body: JSON.stringify({ value: String(setting.value) })
          })
        )
      }

      // Create missing settings via POST with preset values
      for (const key of missingKeys) {
        const meta = SETTING_METADATA[key]
        // Use the preset value directly
        const presetValue = Object.entries(preset.settings).find(([k]) => k === key)?.[1] || meta?.defaultValue || ''
        
        allPromises.push(
          fetch(`${API_URL}/admin/settings`, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${authStore.token}`
            },
            body: JSON.stringify({
              key,
              displayName: meta?.displayName ?? key,
              value: String(presetValue),
              description: '',
              type: meta?.type ?? 'Text',
              category: meta?.category ?? 'General',
              sortOrder: meta?.sortOrder ?? 0,
              isRequired: false
            })
          }).then(async (response) => {
            // Ignore 409 conflicts (setting already exists) - it will be updated next time
            if (response.status === 409) {
              logDebug(`Setting '${key}' already exists, skipping creation`)
              return new Response(JSON.stringify({ ok: true }), { status: 200 })
            }
            return response
          })
        )
      }

      const results = await Promise.all(allPromises)
      const failedResults = results.filter(r => !r.ok)
      
      if (failedResults.length > 0) {
        throw new Error(`${failedResults.length} settings failed to save`)
      }
      
      toast.success(`✨ ${preset.name} theme preset applied and saved successfully!`)
      
      // Reload settings one final time to reflect all changes
      await loadSettings()
    } catch (err) {
      logError('Error applying theme preset:', err)
      toast.error('Failed to apply theme preset')
    }
  }, `preset-${preset.name}`)
}

// Check if a preset is currently active
const isPresetActive = (preset: typeof themePresets[0]) => {
  const activePresetName = getSetting('theme_preset_active').value
  return activePresetName === preset.name
}

// Filter presets based on active tab
const filteredPresets = computed(() => {
  return themePresets.filter(preset => preset.category === activePresetTab.value)
})

// Detect which provider is selected based on current host value
const detectSmtpProvider = () => {
  const currentHost = getSetting('smtp_host').value
  const provider = smtpProviders.find(p => p.host === currentHost)
  if (provider) {
    smtpHostProvider.value = provider.host
  } else {
    smtpHostProvider.value = 'custom'
    customSmtpHost.value = currentHost
  }
}

// Update SMTP host when provider changes
const handleSmtpProviderChange = () => {
  if (smtpHostProvider.value !== 'custom') {
    const provider = smtpProviders.find(p => p.host === smtpHostProvider.value)
    if (provider) {
      getSetting('smtp_host').value = provider.host
      getSetting('smtp_port').value = provider.port
    }
  } else {
    getSetting('smtp_host').value = customSmtpHost.value
  }
}

// Update host value when custom input changes
const handleCustomHostChange = () => {
  if (smtpHostProvider.value === 'custom') {
    getSetting('smtp_host').value = customSmtpHost.value
  }
}

const tabs = [
  { id: 'general', label: 'General', icon: '⚙️' },
  { id: 'theme', label: 'Theme & Design', icon: '🎨' },
  { id: 'social', label: 'Social Media', icon: '🔗' },
  { id: 'content', label: 'Content', icon: '📝' },
  { id: 'pages', label: 'Pages', icon: '📄' },
  { id: 'email', label: 'Email', icon: '📧' },
  { id: 'advanced', label: 'Advanced', icon: '🔧' }
]

const getSetting = (key: string) => {
  return settings.value.find(s => s.key === key) || { 
    id: 0, 
    key: key, 
    value: '', 
    displayName: '',
    type: 'Text',
    category: 'General',
    sortOrder: 0,
    isRequired: false
  } as SiteSetting
}

// Metadata for settings that may not yet exist in the DB (used when creating via POST)
const SETTING_METADATA: Record<string, { displayName: string; type: string; category: string; sortOrder: number; defaultValue?: string }> = {
  logo_header_height: { displayName: 'Header Logo Height', type: 'Number', category: 'General', sortOrder: 1, defaultValue: '40' },
  logo_footer_height: { displayName: 'Footer Logo Height', type: 'Number', category: 'General', sortOrder: 2, defaultValue: '48' },
  facebook_url: { displayName: 'Facebook URL', type: 'Url', category: 'Social Media', sortOrder: 1 },
  instagram_url: { displayName: 'Instagram URL', type: 'Url', category: 'Social Media', sortOrder: 2 },
  twitter_url: { displayName: 'Twitter URL', type: 'Url', category: 'Social Media', sortOrder: 3 },
  youtube_url: { displayName: 'YouTube URL', type: 'Url', category: 'Social Media', sortOrder: 4 },
  smtp_enabled: { displayName: 'Enable Email (SMTP)', type: 'Boolean', category: 'Email', sortOrder: 1 },
  smtp_host: { displayName: 'SMTP Host', type: 'Text', category: 'Email', sortOrder: 2 },
  smtp_port: { displayName: 'SMTP Port', type: 'Number', category: 'Email', sortOrder: 3 },
  smtp_username: { displayName: 'SMTP Username', type: 'Text', category: 'Email', sortOrder: 4 },
  smtp_password: { displayName: 'SMTP Password', type: 'Text', category: 'Email', sortOrder: 5 },
  smtp_from_email: { displayName: 'From Email Address', type: 'Email', category: 'Email', sortOrder: 6 },
  smtp_from_name: { displayName: 'From Name', type: 'Text', category: 'Email', sortOrder: 7 },
  smtp_use_ssl: { displayName: 'Use SSL/TLS', type: 'Boolean', category: 'Email', sortOrder: 8 },
  site_url: { displayName: 'Site URL', type: 'Url', category: 'Email', sortOrder: 9 },

  // Music Player
  music_enabled: { displayName: 'Enable Music Player', type: 'Boolean', category: 'Music', sortOrder: 1, defaultValue: 'false' },
  music_embed_code: { displayName: 'Embed Code', type: 'TextArea', category: 'Music', sortOrder: 2, defaultValue: '' },
  
  // Theme - Preset Tracking
  theme_preset_active: { displayName: 'Active Theme Preset', type: 'Text', category: 'Theme', sortOrder: 99, defaultValue: '' },
  
  // Theme - Brand Colors
  theme_primary_color: { displayName: 'Primary Color', type: 'Color', category: 'Theme', sortOrder: 100, defaultValue: '#4b5563' },
  theme_secondary_color: { displayName: 'Secondary Color', type: 'Color', category: 'Theme', sortOrder: 101, defaultValue: '#6b7280' },
  theme_accent_color: { displayName: 'Accent Color', type: 'Color', category: 'Theme', sortOrder: 102, defaultValue: '#3b82f6' },
  theme_success_color: { displayName: 'Success Color', type: 'Color', category: 'Theme', sortOrder: 103, defaultValue: '#10b981' },
  theme_warning_color: { displayName: 'Warning Color', type: 'Color', category: 'Theme', sortOrder: 104, defaultValue: '#f59e0b' },
  theme_danger_color: { displayName: 'Danger Color', type: 'Color', category: 'Theme', sortOrder: 105, defaultValue: '#ef4444' },
  
  // Theme - Backgrounds
  theme_bg_color: { displayName: 'Page Background', type: 'Color', category: 'Theme', sortOrder: 110, defaultValue: '#ffffff' },
  theme_bg_secondary: { displayName: 'Secondary Background', type: 'Color', category: 'Theme', sortOrder: 111, defaultValue: '#f9fafb' },
  theme_bg_muted: { displayName: 'Muted Background', type: 'Color', category: 'Theme', sortOrder: 112, defaultValue: '#f3f4f6' },
  
  // Theme - Text
  theme_text_primary: { displayName: 'Primary Text', type: 'Color', category: 'Theme', sortOrder: 120, defaultValue: '#111827' },
  theme_text_secondary: { displayName: 'Secondary Text', type: 'Color', category: 'Theme', sortOrder: 121, defaultValue: '#6b7280' },
  theme_text_muted: { displayName: 'Muted Text', type: 'Color', category: 'Theme', sortOrder: 122, defaultValue: '#9ca3af' },
  
  // Theme - Borders
  theme_border_color: { displayName: 'Default Border', type: 'Color', category: 'Theme', sortOrder: 130, defaultValue: '#e5e7eb' },
  theme_border_accent: { displayName: 'Accent Border', type: 'Color', category: 'Theme', sortOrder: 131, defaultValue: '#d1d5db' },
  
  // Theme - Typography
  theme_font_heading: { displayName: 'Heading Font', type: 'Text', category: 'Theme', sortOrder: 200, defaultValue: "'Inter', system-ui, sans-serif" },
  theme_font_body: { displayName: 'Body Font', type: 'Text', category: 'Theme', sortOrder: 201, defaultValue: "'Inter', system-ui, sans-serif" },
  theme_font_size_base: { displayName: 'Base Font Size', type: 'Number', category: 'Theme', sortOrder: 210, defaultValue: '16' },
  theme_font_size_h1: { displayName: 'H1 Size', type: 'Number', category: 'Theme', sortOrder: 211, defaultValue: '2.5' },
  theme_font_size_h2: { displayName: 'H2 Size', type: 'Number', category: 'Theme', sortOrder: 212, defaultValue: '2' },
  theme_font_size_h3: { displayName: 'H3 Size', type: 'Number', category: 'Theme', sortOrder: 213, defaultValue: '1.5' },
  theme_font_weight_heading: { displayName: 'Heading Weight', type: 'Number', category: 'Theme', sortOrder: 220, defaultValue: '700' },
  theme_font_weight_body: { displayName: 'Body Weight', type: 'Number', category: 'Theme', sortOrder: 221, defaultValue: '400' },
  theme_line_height_heading: { displayName: 'Heading Line Height', type: 'Number', category: 'Theme', sortOrder: 230, defaultValue: '1.2' },
  theme_line_height_body: { displayName: 'Body Line Height', type: 'Number', category: 'Theme', sortOrder: 231, defaultValue: '1.6' },
  
  // Theme - Buttons
  theme_button_radius: { displayName: 'Button Border Radius', type: 'Number', category: 'Theme', sortOrder: 300, defaultValue: '6' },
  theme_button_padding_y: { displayName: 'Button Padding Y', type: 'Number', category: 'Theme', sortOrder: 301, defaultValue: '0.75' },
  theme_button_padding_x: { displayName: 'Button Padding X', type: 'Number', category: 'Theme', sortOrder: 302, defaultValue: '1.5' },
  theme_button_font_weight: { displayName: 'Button Font Weight', type: 'Number', category: 'Theme', sortOrder: 303, defaultValue: '600' },
  theme_button_text_transform: { displayName: 'Button Text Transform', type: 'Text', category: 'Theme', sortOrder: 304, defaultValue: 'none' },
  
  // Theme - Cards
  theme_card_radius: { displayName: 'Card Border Radius', type: 'Number', category: 'Theme', sortOrder: 310, defaultValue: '12' },
  theme_card_padding: { displayName: 'Card Padding', type: 'Number', category: 'Theme', sortOrder: 311, defaultValue: '1.5' },
  theme_card_shadow: { displayName: 'Card Shadow', type: 'Text', category: 'Theme', sortOrder: 312, defaultValue: '0 1px 3px 0 rgb(0 0 0 / 0.1)' },
  
  // Theme - Inputs
  theme_input_radius: { displayName: 'Input Border Radius', type: 'Number', category: 'Theme', sortOrder: 320, defaultValue: '6' },
  theme_input_border_width: { displayName: 'Input Border Width', type: 'Number', category: 'Theme', sortOrder: 321, defaultValue: '1' },
  theme_input_focus_color: { displayName: 'Input Focus Color', type: 'Color', category: 'Theme', sortOrder: 322, defaultValue: '#6366f1' },
  
  // Theme - Layout
  theme_container_max_width: { displayName: 'Max Container Width', type: 'Number', category: 'Theme', sortOrder: 400, defaultValue: '1280' },
  theme_container_padding: { displayName: 'Container Padding', type: 'Number', category: 'Theme', sortOrder: 401, defaultValue: '20' },
  theme_section_padding_top: { displayName: 'Section Padding Top', type: 'Number', category: 'Theme', sortOrder: 410, defaultValue: '4' },
  theme_section_padding_bottom: { displayName: 'Section Padding Bottom', type: 'Number', category: 'Theme', sortOrder: 411, defaultValue: '4' },
  theme_element_gap: { displayName: 'Element Gap', type: 'Number', category: 'Theme', sortOrder: 412, defaultValue: '1.5' },
  
  // Theme - Effects
  theme_transition_duration: { displayName: 'Transition Duration', type: 'Number', category: 'Theme', sortOrder: 500, defaultValue: '200' },
  theme_transition_timing: { displayName: 'Transition Timing', type: 'Text', category: 'Theme', sortOrder: 501, defaultValue: 'ease' },
  theme_hover_lift_enabled: { displayName: 'Enable Hover Lift', type: 'Boolean', category: 'Theme', sortOrder: 510, defaultValue: 'true' },
  theme_hover_lift_amount: { displayName: 'Hover Lift Amount', type: 'Number', category: 'Theme', sortOrder: 511, defaultValue: '4' },
  theme_hover_scale: { displayName: 'Hover Scale', type: 'Number', category: 'Theme', sortOrder: 512, defaultValue: '1.02' },
  theme_hover_shadow: { displayName: 'Hover Shadow', type: 'Text', category: 'Theme', sortOrder: 513, defaultValue: '0 8px 20px 0 rgb(0 0 0 / 0.15)' },
  
  // Theme - Header
  theme_header_bg: { displayName: 'Header Background', type: 'Color', category: 'Theme', sortOrder: 600, defaultValue: '#ffffff' },
  theme_header_text: { displayName: 'Header Text Color', type: 'Color', category: 'Theme', sortOrder: 601, defaultValue: '#111827' },
  theme_header_height: { displayName: 'Header Height', type: 'Number', category: 'Theme', sortOrder: 602, defaultValue: '72' },
  theme_header_sticky: { displayName: 'Header Sticky', type: 'Boolean', category: 'Theme', sortOrder: 603, defaultValue: 'true' },
  theme_header_shadow: { displayName: 'Header Shadow', type: 'Text', category: 'Theme', sortOrder: 604, defaultValue: '0 1px 3px 0 rgb(0 0 0 / 0.1)' },
  
  // Theme - Footer
  theme_footer_bg: { displayName: 'Footer Background', type: 'Color', category: 'Theme', sortOrder: 700, defaultValue: '#1f2937' },
  theme_footer_text: { displayName: 'Footer Text Color', type: 'Color', category: 'Theme', sortOrder: 701, defaultValue: '#9ca3af' },
  theme_footer_padding: { displayName: 'Footer Padding', type: 'Number', category: 'Theme', sortOrder: 702, defaultValue: '3' },
  
  // Theme - Advanced
  theme_dark_mode_enabled: { displayName: 'Enable Dark Mode Toggle', type: 'Boolean', category: 'Theme', sortOrder: 800, defaultValue: 'false' },
  theme_smooth_scroll: { displayName: 'Enable Smooth Scrolling', type: 'Boolean', category: 'Theme', sortOrder: 801, defaultValue: 'true' },
  theme_parallax_enabled: { displayName: 'Enable Parallax Effects', type: 'Boolean', category: 'Theme', sortOrder: 802, defaultValue: 'false' },
  theme_glass_morphism: { displayName: 'Glass Morphism', type: 'Boolean', category: 'Theme', sortOrder: 803, defaultValue: 'false' },
  theme_animations_enabled: { displayName: 'Page Animations', type: 'Boolean', category: 'Theme', sortOrder: 804, defaultValue: 'true' },
  theme_gradient_overlays: { displayName: 'Gradient Overlays', type: 'Boolean', category: 'Theme', sortOrder: 805, defaultValue: 'false' },
  theme_custom_css: { displayName: 'Custom CSS', type: 'Text', category: 'Theme', sortOrder: 900, defaultValue: '' },
  
  // Theme - Visual Effects
  theme_button_style: { displayName: 'Button Style', type: 'Text', category: 'Theme', sortOrder: 1000, defaultValue: 'solid' },
  theme_corner_style: { displayName: 'Corner Style', type: 'Text', category: 'Theme', sortOrder: 1001, defaultValue: 'rounded' },
  theme_image_hover: { displayName: 'Image Hover Effect', type: 'Text', category: 'Theme', sortOrder: 1010, defaultValue: 'zoom' },
  theme_image_border_style: { displayName: 'Image Border Style', type: 'Text', category: 'Theme', sortOrder: 1011, defaultValue: 'shadow' },
  theme_bg_pattern: { displayName: 'Background Pattern', type: 'Text', category: 'Theme', sortOrder: 1020, defaultValue: 'none' },
  theme_heading_shadow: { displayName: 'Heading Text Shadow', type: 'Text', category: 'Theme', sortOrder: 1030, defaultValue: 'none' },
  theme_letter_spacing: { displayName: 'Letter Spacing', type: 'Text', category: 'Theme', sortOrder: 1031, defaultValue: 'normal' },
  theme_loading_animation: { displayName: 'Loading Animation', type: 'Text', category: 'Theme', sortOrder: 1040, defaultValue: 'spinner' },
  
  // Theme - Gradients
  theme_gradient_start: { displayName: 'Gradient Start Color', type: 'Color', category: 'Theme', sortOrder: 1100, defaultValue: '#4b5563' },
  theme_gradient_end: { displayName: 'Gradient End Color', type: 'Color', category: 'Theme', sortOrder: 1101, defaultValue: '#3b82f6' },
  theme_gradient_direction: { displayName: 'Gradient Direction', type: 'Text', category: 'Theme', sortOrder: 1102, defaultValue: 'to right' },
  theme_gradient_opacity: { displayName: 'Gradient Opacity', type: 'Number', category: 'Theme', sortOrder: 1103, defaultValue: '70' },
  theme_backdrop_blur: { displayName: 'Backdrop Blur', type: 'Number', category: 'Theme', sortOrder: 1110, defaultValue: '10' },
  theme_modal_backdrop_opacity: { displayName: 'Modal Backdrop Opacity', type: 'Number', category: 'Theme', sortOrder: 1111, defaultValue: '75' },
  
  // Content - Partner Brands
  partner_brands: { displayName: 'Partner Brands', type: 'Text', category: 'Content', sortOrder: 100, defaultValue: '[]' },
  brands_section_title: { displayName: 'Brands Section Title', type: 'Text', category: 'Content', sortOrder: 101, defaultValue: 'Brands We Carry' },
  brands_section_subtitle: { displayName: 'Brands Section Subtitle', type: 'Text', category: 'Content', sortOrder: 102, defaultValue: 'We partner with industry-leading manufacturers to bring you the best powersports vehicles and equipment.' },
  
  // Security & Advanced
  session_timeout: { displayName: 'Session Timeout (minutes)', type: 'Number', category: 'Advanced', sortOrder: 1, defaultValue: '480' },
  max_login_attempts: { displayName: 'Max Login Attempts', type: 'Number', category: 'Advanced', sortOrder: 2, defaultValue: '5' },
  allow_user_registration: { displayName: 'Allow User Registration', type: 'Boolean', category: 'Advanced', sortOrder: 3, defaultValue: 'true' },
  require_email_verification: { displayName: 'Require Email Verification', type: 'Boolean', category: 'Advanced', sortOrder: 4, defaultValue: 'true' },
  enable_two_factor_auth: { displayName: 'Enable Two-Factor Authentication', type: 'Boolean', category: 'Advanced', sortOrder: 5, defaultValue: 'false' },
  allow_guest_checkout: { displayName: 'Allow Guest Checkout', type: 'Boolean', category: 'Advanced', sortOrder: 6, defaultValue: 'true' },
}

const handleToggleChange = (key: string, event: Event) => {
  const target = event.target as HTMLInputElement
  getSetting(key).value = target.checked ? 'true' : 'false'
}

const loadSettings = async () => {
  await executeWithLoading(async () => {
    error.value = ''
    
    try {
      logDebug('Loading settings from admin API', { url: `${API_URL}/admin/settings` });
      
      const data = await apiGet<SiteSetting[]>('/admin/settings')
      
      logDebug('Settings loaded successfully', { count: data.length });
      settings.value = data
      
      if (settings.value.length === 0) {
        error.value = 'No settings found in database. They should be seeded on startup.'
      }
    } catch (err: any) {
      logError('Failed to load settings', err);
      error.value = err.message || 'Failed to load settings'
    }
  })
}

// Media Picker functions
const openMediaPickerFor = (settingKey: string) => {
  mediaPickerTargetSetting.value = settingKey
  showMediaPicker.value = true
}

const handleMediaSelection = (mediaFile: any) => {
  if (mediaPickerTargetSetting.value) {
    getSetting(mediaPickerTargetSetting.value).value = mediaFile.filePath
    showMediaPicker.value = false
    mediaPickerTargetSetting.value = null
  }
}

const saveSection = async (settingKeys: string[], sectionName: string) => {
  await executeWithLoading(async () => {
    try {
      logDebug(`Saving ${sectionName} section`, { keys: settingKeys });
      
      // Split into existing settings (PUT) and missing ones (POST to create)
      const settingsToUpdate = settings.value.filter(s => settingKeys.includes(s.key) && s.id > 0)
      const missingKeys = settingKeys.filter(k => !settings.value.some(s => s.key === k && s.id > 0))
      
      if (settingsToUpdate.length === 0 && missingKeys.length === 0) {
        throw new Error('No valid settings to save in this section')
      }
      
      logDebug('Settings to save', { 
        updating: settingsToUpdate.length, 
        creating: missingKeys.length 
      });
      
      const allPromises: Promise<any>[] = []

      // Update existing settings via PUT
      for (const setting of settingsToUpdate) {
        allPromises.push(
          apiPut(`/admin/settings/${setting.id}`, { value: String(setting.value) })
        )
      }

      // Create missing settings via POST
      for (const key of missingKeys) {
        const meta = SETTING_METADATA[key]
        let currentValue = getSetting(key).value
        
        // Use default value if current value is empty and a default is defined
        if (!currentValue && meta?.defaultValue) {
          currentValue = meta.defaultValue
        }
        
        allPromises.push(
          apiPost('/admin/settings', {
            key,
            displayName: meta?.displayName ?? key,
            value: String(currentValue),
            description: '',
            type: meta?.type ?? 'Text',
            category: meta?.category ?? 'General',
            sortOrder: meta?.sortOrder ?? 0,
            isRequired: false
          })
        )
      }

      await Promise.all(allPromises)

      // Show success toast notification
      const sectionDisplayNames: Record<string, string> = {
        general: 'General',
        social: 'Social media',
        homepage: 'Homepage',
        products: 'Products page',
        about: 'About page',
        contact: 'Contact page',
        faq: 'FAQ page',
        shipping: 'Shipping & Returns page',
        privacy: 'Privacy Policy page',
        terms: 'Terms of Service page',
        smtp: 'SMTP',
        security: 'Security & Access',
        performance: 'Performance & Caching',
        system: 'System',
        theme: 'Theme & Design'
      }
      toast.saveSuccess(`${sectionDisplayNames[sectionName] || sectionName} settings`)
      
      // Reload settings to get updated values (including newly created IDs)
      await loadSettings()
    } catch (err: any) {
      logError('Failed to save settings', err);
      toast.saveError(err.message || 'Failed to save settings')
    }
  }, `save-${sectionName}`)
}

const sendTestEmail = async () => {
  if (!testEmailAddress.value) {
    testEmailError.value = 'Please enter an email address.'
    return
  }
  await executeWithLoading(async () => {
    testEmailSuccess.value = ''
    testEmailError.value = ''
    try {
      const data = await apiPost(`/admin/settings/test-email?email=${encodeURIComponent(testEmailAddress.value)}`)
      testEmailSuccess.value = data.message || 'Test email sent successfully!'
      setTimeout(() => testEmailSuccess.value = '', SUCCESS_MESSAGE_DURATION_MS)
    } catch (err: any) {
      testEmailError.value = err.message || 'Failed to send test email'
      setTimeout(() => testEmailError.value = '', 8000)
    }
  }, 'sendTestEmail')
}

const resetSettings = async () => {
  if (!confirm('Are you sure you want to reset all settings to defaults? This will overwrite all current values.')) {
    return
  }

  await executeWithLoading(async () => {
    error.value = ''
    
    try {
      logDebug('Resetting settings to defaults');
      
      const result = await apiPost('/admin/settings/reset')
      logDebug('Settings reset successful', result);
      
      // Reload settings
      await loadSettings()
      
      toast.success('Settings have been reset to defaults successfully!')
    } catch (err: any) {
      logError('Failed to reset settings', err);
      error.value = err.message || 'Failed to reset settings'
    }
  }, 'resetSettings')
}

const confirmResetTheme = async () => {
  if (!confirm('⚠️ Are you sure you want to reset ALL theme settings to their default values?\n\nThis will restore colors, typography, spacing, effects, header, footer, and all advanced options to defaults.\n\nThis action cannot be undone!')) {
    return
  }

  const themeKeys = [
    // Preset
    'theme_preset_active',
    // Colors
    'theme_primary_color', 'theme_secondary_color', 'theme_accent_color',
    'theme_success_color', 'theme_warning_color', 'theme_danger_color',
    'theme_bg_color', 'theme_bg_secondary', 'theme_bg_muted',
    'theme_text_primary', 'theme_text_secondary', 'theme_text_muted',
    'theme_border_color', 'theme_border_accent',
    // Typography
    'theme_font_heading', 'theme_font_body',
    'theme_font_size_base', 'theme_font_size_h1', 'theme_font_size_h2', 'theme_font_size_h3',
    'theme_font_weight_heading', 'theme_font_weight_body',
    'theme_line_height_heading', 'theme_line_height_body',
    // Components
    'theme_button_radius', 'theme_button_padding_y', 'theme_button_padding_x',
    'theme_button_font_weight', 'theme_button_text_transform',
    'theme_card_radius', 'theme_card_padding', 'theme_card_shadow',
    'theme_input_radius', 'theme_input_border_width', 'theme_input_focus_color',
    // Layout
    'theme_container_max_width', 'theme_container_padding',
    'theme_section_padding_top', 'theme_section_padding_bottom', 'theme_element_gap',
    // Effects
    'theme_transition_duration', 'theme_transition_timing',
    'theme_hover_lift_enabled', 'theme_hover_lift_amount', 'theme_hover_scale', 'theme_hover_shadow',
    // Header
    'theme_header_bg', 'theme_header_text', 'theme_header_height', 'theme_header_sticky', 'theme_header_shadow',
    // Footer
    'theme_footer_bg', 'theme_footer_text', 'theme_footer_padding',
    // Advanced
    'theme_dark_mode_enabled', 'theme_smooth_scroll', 'theme_parallax_enabled',
    'theme_glass_morphism', 'theme_animations_enabled', 'theme_gradient_overlays', 'theme_custom_css',
    // Visual Effects
    'theme_button_style', 'theme_corner_style', 'theme_image_hover', 'theme_image_border_style',
    'theme_bg_pattern', 'theme_heading_shadow', 'theme_letter_spacing', 'theme_loading_animation',
    // Gradients
    'theme_gradient_start', 'theme_gradient_end', 'theme_gradient_direction', 'theme_gradient_opacity',
    'theme_backdrop_blur', 'theme_modal_backdrop_opacity'
  ]

  await executeWithLoading(async () => {
    try {
      // Reset each theme setting to its default value
      for (const key of themeKeys) {
        const setting = getSetting(key)
        const metadata = SETTING_METADATA[key]
        if (metadata && metadata.defaultValue !== undefined) {
          setting.value = metadata.defaultValue
        }
      }

      // Save all theme settings
      await saveSection(themeKeys, 'theme')
      
      toast.success('Theme settings have been reset to defaults!')
    } catch (err: any) {
      logError('Failed to reset theme', err)
      toast.error('Failed to reset theme settings')
    }
  }, 'resetTheme')
}

onMounted(async () => {
  await loadSettings()
  detectSmtpProvider()
})
</script>

<style scoped>
.settings-page {
  padding: 2rem;
  max-width: 1000px;
  margin: 0 auto;
}

.page-title {
  font-size: 2rem;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 2rem 0;
}

.tabs {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 2rem;
  border-bottom: 2px solid #e5e7eb;
}

.tab {
  padding: 0.875rem 1.5rem;
  background: none;
  border: none;
  border-bottom: 3px solid transparent;
  color: #6b7280;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: -2px;
}

.tab:hover {
  color: #4f46e5;
  background: rgba(79, 70, 229, 0.05);
}

.tab.active {
  color: #4f46e5;
  border-bottom-color: #4f46e5;
}

.tab-icon {
  font-size: 1.25rem;
}

.loading-state, .error-state {
  text-align: center;
  padding: 4rem 2rem;
}

.error-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-top: 1.5rem;
}

.spinner {
  border: 3px solid #f3f4f6;
  border-top: 3px solid #4f46e5;
  border-radius: 50%;
  width: 48px;
  height: 48px;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.settings-form {
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 2rem;
}

.tab-content {
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.content-section {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 2rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  transition: box-shadow 0.2s;
}

.content-section:hover {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.content-section:last-child {
  margin-bottom: 0;
}

/* Subsections within content sections */
.subsection {
  margin-bottom: 2rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid #f3f4f6;
}

.subsection:last-child {
  margin-bottom: 0;
  padding-bottom: 0;
  border-bottom: none;
}

.subsection-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #374151;
  margin: 0 0 1.25rem 0;
  padding-bottom: 0.75rem;
  border-bottom: 2px solid #e5e7eb;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.settings-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  transition: box-shadow 0.2s;
}

.settings-card:hover {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.content-section .form-actions,
.settings-card .form-actions {
  margin-top: 1rem;
  padding-top: 0;
  border-top: none;
  justify-content: flex-start;
}

.content-section .alert,
.settings-card .alert {
  margin-top: 1rem;
  margin-bottom: 0;
}

.section-header {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 1.5rem 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

/* Collapsible Section Styles */
.section-header-collapsible {
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  padding: 0.75rem;
  margin: -0.75rem -1rem 1rem -1rem;
  border-radius: 8px;
  transition: all 0.2s ease;
  user-select: none;
}

.section-header-collapsible:hover {
  background: #f9fafb;
}

.section-header-collapsible .section-header {
  margin: 0;
  flex: 1;
}

.collapse-icon {
  font-size: 0.875rem;
  color: #6b7280;
  font-weight: bold;
  transition: transform 0.3s ease;
  min-width: 20px;
  text-align: center;
}

.collapsible-content {
  animation: slideDown 0.3s ease;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.section-description {
  font-size: 0.938rem;
  color: #6b7280;
  margin: -0.75rem 0 1.5rem 0;
  line-height: 1.6;
}

/* Theme Presets Section */
.theme-presets-section {
  margin-bottom: 1.5rem;
}

.preset-tabs {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1.5rem;
  border-bottom: 2px solid #e5e7eb;
}

.preset-tab {
  flex: 1;
  padding: 0.75rem 1.5rem;
  background: transparent;
  border: none;
  border-bottom: 3px solid transparent;
  color: #6b7280;
  font-size: 0.938rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  margin-bottom: -2px;
}

.preset-tab:hover {
  color: #4f46e5;
  background: #f9fafb;
}

.preset-tab.active {
  color: #4f46e5;
  border-bottom-color: #4f46e5;
  background: transparent;
}

.theme-presets-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
}

.theme-preset-card {
  position: relative;
  background: linear-gradient(135deg, #f9fafb 0%, #ffffff 100%);
  border: 2px solid #e5e7eb;
  border-radius: 12px;
  padding: 1.5rem;
  cursor: pointer;
  transition: all 0.3s ease;
  overflow: hidden;
}

.theme-preset-card:hover {
  border-color: #4f46e5;
  box-shadow: 0 8px 20px rgba(79, 70, 229, 0.15);
  transform: translateY(-4px);
}

.theme-preset-card:hover .preset-apply-overlay {
  opacity: 1;
}

.theme-preset-card.active {
  border-color: #10b981;
  background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.2);
}

.preset-active-badge {
  position: absolute;
  top: 12px;
  right: 12px;
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
  font-size: 0.75rem;
  font-weight: 700;
  padding: 0.375rem 0.75rem;
  border-radius: 6px;
  z-index: 5;
  box-shadow: 0 2px 8px rgba(16, 185, 129, 0.4);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.preset-icon {
  font-size: 2.5rem;
  margin-bottom: 0.75rem;
  text-align: center;
}

.preset-name {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 0.5rem 0;
  text-align: center;
}

.preset-description {
  font-size: 0.875rem;
  color: #6b7280;
  margin: 0 0 1rem 0;
  text-align: center;
  min-height: 2.5rem;
}

.preset-preview-colors {
  display: flex;
  gap: 0.5rem;
  justify-content: center;
}

.preview-color-dot {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 2px solid #ffffff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  cursor: help;
  transition: transform 0.2s;
}

.preview-color-dot:hover {
  transform: scale(1.2);
}

.preset-apply-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(79, 70, 229, 0.95) 0%, rgba(139, 92, 246, 0.95) 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 1.125rem;
  opacity: 0;
  transition: opacity 0.3s ease;
  gap: 0.5rem;
}

.preset-apply-overlay .icon {
  font-size: 1.5rem;
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-group.full-width {
  grid-column: 1 / -1;
}

.form-label {
  display: block;
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: #374151;
  font-size: 0.938rem;
}

.form-label.required::after {
  content: ' *';
  color: #ef4444;
}

.form-control {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.938rem;
  transition: all 0.2s;
}

.form-control:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.form-control.html-editor {
  font-family: 'Courier New', Courier, monospace;
  font-size: 0.875rem;
  line-height: 1.6;
  background: #f9fafb;
  resize: vertical;
  min-height: 200px;
}

.form-hint {
  font-size: 0.813rem;
  color: #6b7280;
  margin: 0.375rem 0 0 0;
}

.coming-soon-badge {
  display: inline-block;
  background: #f59e0b;
  color: white;
  font-size: 0.688rem;
  font-weight: 600;
  padding: 0.188rem 0.5rem;
  border-radius: 4px;
  margin-left: 0.5rem;
  vertical-align: middle;
  text-transform: uppercase;
  letter-spacing: 0.025em;
}

/* Backup Section Styles */
.backup-actions-grid {
  display: grid;
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.backup-action-card {
  background: linear-gradient(135deg, #f9fafb 0%, #ffffff 100%);
  border: 2px solid #e5e7eb;
  border-radius: 12px;
  padding: 2rem;
  text-align: center;
  transition: all 0.3s ease;
}

.backup-action-card:hover {
  border-color: #4f46e5;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.15);
}

.backup-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.backup-action-card h4 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 0.5rem 0;
}

.backup-action-card p {
  color: #6b7280;
  margin: 0 0 1.5rem 0;
}

.backup-status {
  margin-top: 1rem;
  padding: 0.75rem;
  border-radius: 6px;
  font-size: 0.938rem;
  font-weight: 500;
}

.backup-status.success {
  background: #d1fae5;
  color: #065f46;
  border: 1px solid #6ee7b7;
}

.backup-status.error {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.backup-status.info {
  background: #dbeafe;
  color: #1e40af;
  border: 1px solid #93c5fd;
}

.recent-backups-section {
  margin-top: 2rem;
  padding-top: 2rem;
  border-top: 2px solid #e5e7eb;
}

.backups-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 1rem;
}

.backup-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem;
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  transition: all 0.2s ease;
}

.backup-item:hover {
  background: #ffffff;
  border-color: #d1d5db;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.backup-info {
  flex: 1;
}

.backup-name {
  font-weight: 600;
  color: #1f2937;
  margin-bottom: 0.5rem;
  font-family: 'Courier New', monospace;
  font-size: 0.938rem;
}

.backup-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.875rem;
  color: #6b7280;
}

.backup-actions {
  display: flex;
  gap: 0.5rem;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: #6b7280;
}

.empty-state .icon {
  font-size: 3rem;
  display: block;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.loading-state {
  text-align: center;
  padding: 2rem;
  color: #6b7280;
}

.spinner {
  display: inline-block;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.alert {
  padding: 1rem 1.25rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
  font-weight: 500;
}

.alert-success {
  background: #d1fae5;
  color: #065f46;
  border: 1px solid #6ee7b7;
}

.alert-danger {
  background: #fee2e2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.form-actions {
  display: flex;
  justify-content: center;
  padding-top: 1.5rem;
  border-top: 1px solid #e5e7eb;
}

.btn {
  padding: 0.625rem 1.25rem;
  border: none;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background: #4f46e5;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #4338ca;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
}

.btn-secondary {
  background: #6b7280;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background: #4b5563;
}

.btn-danger {
  background: #ef4444;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background: #dc2626;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
}

.btn-lg {
  padding: 0.875rem 2rem;
  font-size: 1rem;
}

.icon {
  font-size: 1.125rem;
}

@media (max-width: 768px) {
  .settings-page {
    padding: 1rem;
  }

  .settings-grid {
    grid-template-columns: 1fr;
  }

  .theme-presets-grid {
    grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  }

  .tabs {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .tab {
    padding: 0.75rem 1rem;
    white-space: nowrap;
  }
}

/* Image Preview */
.current-setting-image {
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  gap: 1rem;
}

.current-setting-image img {
  width: 150px;
  height: 150px;
  object-fit: cover;
  border-radius: 8px;
  border: 2px solid #e5e7eb;
}

/* Color Picker Styles */
.color-input-group {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.color-picker {
  width: 60px;
  height: 42px;
  padding: 0.25rem;
  border: 2px solid #d1d5db;
  border-radius: 6px;
  cursor: pointer;
  transition: border-color 0.2s;
}

.color-picker:hover {
  border-color: #4f46e5;
}

.color-picker:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.color-input-group .form-control {
  flex: 1;
  font-family: 'Courier New', monospace;
  text-transform: uppercase;
}

/* Theme-Specific Grid */
.color-grid {
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
}

/* Subsection Styles */
.subsection {
  margin-bottom: 2rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid #e5e7eb;
}

.subsection:last-of-type {
  border-bottom: none;
  margin-bottom: 1.5rem;
  padding-bottom: 0;
}

.subsection-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #374151;
  margin: 0 0 1.25rem 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

/* Full Width Form Groups */
.form-group.full-width {
  grid-column: 1 / -1;
}

.form-group.full-width textarea {
  font-family: 'Courier New', monospace;
  font-size: 0.875rem;
  resize: vertical;
}

/* Alert Styles */
.alert {
  padding: 1rem 1.25rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
  display: flex;
  align-items: start;
  gap: 0.75rem;
}

.alert-warning {
  background: #fef3c7;
  border: 1px solid #fbbf24;
  color: #92400e;
}

.alert strong {
  font-weight: 600;
}

/* Settings Card for Theme Tab */
.settings-card {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  padding: 2rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.section-header {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 1.5rem 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

/* Theme Preview Banner */
/* Button & Preset Grids */
.button-preset-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.preset-option {
  cursor: pointer;
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.preset-option input[type="radio"] {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

.preset-demo {
  background: #f3f4f6;
  border: 3px solid #e5e7eb;
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100px;
  transition: all 0.2s;
}

.preset-option input[type="radio"]:checked + .preset-demo {
  border-color: #4f46e5;
  background: #eef2ff;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.preset-demo:hover {
  border-color: #4f46e5;
}

.preset-label {
  text-align: center;
  font-weight: 600;
  color: #374151;
}

/* Demo Elements */
.demo-button {
  padding: 0.75rem 1.5rem;
  font-weight: 600;
  font-size: 0.875rem;
  transition: all 0.2s;
  cursor: pointer;
}

.solid-style {
  background: #4f46e5;
  color: white;
  border-radius: 6px;
}

.outlined-style {
  background: transparent;
  color: #4f46e5;
  border: 2px solid #4f46e5;
  border-radius: 6px;
}

.raised-style {
  background: #4f46e5;
  color: white;
  border-radius: 6px;
  box-shadow: 0 4px 0 #3730a3;
}

.raised-style:hover {
  transform: translateY(2px);
  box-shadow: 0 2px 0 #3730a3;
}

.ghost-style {
  background: transparent;
  color: #4f46e5;
  border-radius: 6px;
}

.ghost-style:hover {
  background: rgba(79, 70, 229, 0.1);
}

.demo-card {
  width: 100%;
  height: 60px;
  background: white;
  border: 2px solid #e5e7eb;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  color: #6b7280;
}

.sharp-corners {
  border-radius: 0;
}

.rounded-corners {
  border-radius: 8px;
}

.extra-rounded-corners {
  border-radius: 16px;
}

.pill-corners {
  border-radius: 999px;
}

/* Range Slider */
.range-slider {
  width: 100%;
  height: 8px;
  border-radius: 4px;
  background: #e5e7eb;
  outline: none;
  -webkit-appearance: none;
  appearance: none;
}

.range-slider::-webkit-slider-thumb {
  -webkit-appearance: none;
  appearance: none;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: #4f46e5;
  cursor: pointer;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.range-slider::-moz-range-thumb {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: #4f46e5;
  cursor: pointer;
  border: none;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.range-value {
  display: inline-block;
  margin-top: 0.5rem;
  font-weight: 600;
  color: #4f46e5;
  font-size: 0.875rem;
}

/* Gradient Preview */
.gradient-preview {
  width: 100%;
  height: 120px;
  border-radius: 12px;
  margin-top: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  border: 2px solid #e5e7eb;
}

.gradient-preview span {
  color: white;
  font-weight: 700;
  font-size: 1.25rem;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

/* Code Input */
.code-input {
  font-family: 'Courier New', 'Consolas', monospace;
  font-size: 0.875rem;
  line-height: 1.6;
  background: #1e293b;
  color: #e2e8f0;
  border: 2px solid #334155;
}

.code-input:focus {
  background: #0f172a;
  border-color: #4f46e5;
}

.code-input::placeholder {
  color: #64748b;
}

</style>