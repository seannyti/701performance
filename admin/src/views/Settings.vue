<template>
  <AdminLayout>
    <div class="settings-page">
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
          <button @click="resetSettings" class="btn btn-danger">Reset to Defaults</button>
        </div>
      </div>

      <!-- Settings Form -->
      <div v-else class="settings-form">
        <!-- General Tab -->
        <div v-if="activeTab === 'general'" class="tab-content">
          <div class="settings-card">
            <h3 class="section-header">⚙️ General Settings</h3>
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

              <div class="form-group">
                <label class="form-label">Logo URL</label>
                <input 
                  v-model="getSetting('logo_url').value" 
                  type="text" 
                  class="form-control"
                  placeholder="/images/logo.png"
                />
                <p class="form-hint">Path or URL to your logo image</p>
              </div>

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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'general'" class="alert alert-success">
              General settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'general'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <!-- Save Button for General -->
            <div class="form-actions">
              <button @click="saveSection(['site_name', 'site_tagline', 'logo_url', 'contact_email', 'contact_phone', 'contact_address'], 'general')" class="btn btn-primary btn-lg" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save General Settings' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Social Media Tab -->
        <div v-if="activeTab === 'social'" class="tab-content">
          <div class="settings-card">
            <h3 class="section-header">🔗 Social Media Links</h3>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'social'" class="alert alert-success">
              Social media settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'social'" class="alert alert-danger">
              {{ saveError }}
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

        <!-- Content Tab -->
        <div v-if="activeTab === 'content'" class="tab-content">
          <!-- Homepage Section -->
          <div class="content-section">
            <h3 class="section-header">🏠 Homepage</h3>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'homepage'" class="alert alert-success">
              Homepage settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'homepage'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['hero_title', 'hero_subtitle'], 'homepage')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Homepage' }}
              </button>
            </div>
          </div>

          <!-- Products Page Section -->
          <div class="content-section">
            <h3 class="section-header">📦 Products Page</h3>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'products'" class="alert alert-success">
              Products page settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'products'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['products_title', 'products_subtitle'], 'products')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Products Page' }}
              </button>
            </div>
          </div>

          <!-- About Page Section -->
          <div class="content-section">
            <h3 class="section-header">ℹ️ About Page</h3>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'about'" class="alert alert-success">
              About page settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'about'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['about_title', 'about_subtitle'], 'about')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save About Page' }}
              </button>
            </div>
          </div>

          <!-- Contact Page Section -->
          <div class="content-section">
            <h3 class="section-header">📞 Contact Page</h3>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'contact'" class="alert alert-success">
              Contact page settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'contact'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['contact_title', 'contact_subtitle'], 'contact')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Contact Page' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Pages Tab -->
        <div v-if="activeTab === 'pages'" class="tab-content">
          <!-- FAQ Page Section -->
          <div class="content-section">
            <h3 class="section-header">❓ FAQ Page</h3>
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

              <div class="form-group full-width">
                <label class="form-label">Page Content (HTML)</label>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'faq'" class="alert alert-success">
              FAQ page settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'faq'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['faq_title', 'faq_subtitle', 'faq_content'], 'faq')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save FAQ Page' }}
              </button>
            </div>
          </div>

          <!-- Shipping & Returns Page Section -->
          <div class="content-section">
            <h3 class="section-header">📦 Shipping & Returns Page</h3>
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

              <div class="form-group full-width">
                <label class="form-label">Page Content (HTML)</label>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'shipping'" class="alert alert-success">
              Shipping & Returns page settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'shipping'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['shipping_title', 'shipping_subtitle', 'shipping_content'], 'shipping')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Shipping & Returns Page' }}
              </button>
            </div>
          </div>

          <!-- Privacy Policy Page Section -->
          <div class="content-section">
            <h3 class="section-header">🔒 Privacy Policy Page</h3>
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

              <div class="form-group full-width">
                <label class="form-label">Page Content (HTML)</label>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'privacy'" class="alert alert-success">
              Privacy Policy page settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'privacy'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['privacy_title', 'privacy_subtitle', 'privacy_content'], 'privacy')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Privacy Policy Page' }}
              </button>
            </div>
          </div>

          <!-- Terms of Service Page Section -->
          <div class="content-section">
            <h3 class="section-header">📜 Terms of Service Page</h3>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'terms'" class="alert alert-success">
              Terms of Service page settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'terms'" class="alert alert-danger">
              {{ saveError }}
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

        <!-- Advanced Tab -->
        <div v-if="activeTab === 'email'" class="tab-content">
          <!-- SMTP Configuration -->
          <div class="content-section">
            <h3 class="section-header">📧 SMTP Configuration</h3>
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

            <div v-if="sectionSaveSuccess === 'smtp'" class="alert alert-success">
              SMTP settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'smtp'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <div class="form-actions">
              <button @click="saveSection(['smtp_enabled','smtp_host','smtp_port','smtp_username','smtp_password','smtp_from_email','smtp_from_name','smtp_use_ssl','site_url'], 'smtp')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Email Settings' }}
              </button>
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
              <button @click="sendTestEmail" class="btn btn-secondary" :disabled="sendingTestEmail">
                <span class="icon">📤</span>
                {{ sendingTestEmail ? 'Sending...' : 'Send Test Email' }}
              </button>
            </div>
          </div>

          <!-- Email Verification note -->
          <div class="content-section">
            <h3 class="section-header">✅ Email Verification</h3>
            <p class="form-hint" style="font-size: 0.95rem; margin-bottom: 1rem;">
              The <strong>Require Email Verification</strong> toggle is located in the
              <strong>Advanced</strong> tab under Security &amp; Access. Enable it there to require
              new users to verify their email address before logging in.
            </p>
          </div>
        </div>

        <div v-if="activeTab === 'advanced'" class="tab-content">
          <!-- Security & Access Section -->
          <div class="content-section">
            <h3 class="section-header">🔒 Security & Access</h3>
            <div class="settings-grid">
              <div class="form-group">
                <label class="form-label">Session Timeout (minutes)</label>
                <input 
                  v-model="getSetting('session_timeout').value" 
                  type="number" 
                  class="form-control"
                  placeholder="10"
                  min="1"
                  max="1440"
                />
                <p class="form-hint">How long users stay logged in (1-1440 minutes)</p>
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
                </label>
                <p class="form-hint">Require 2FA for enhanced security</p>
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
                </label>
                <p class="form-hint">Let users checkout without creating account</p>
              </div>
            </div>

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'security'" class="alert alert-success">
              Security & Access settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'security'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['session_timeout', 'max_login_attempts', 'allow_user_registration', 'require_email_verification', 'enable_two_factor_auth', 'allow_guest_checkout'], 'security')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Security Settings' }}
              </button>
            </div>
          </div>

          <!-- Performance & Caching Section -->
          <div class="content-section">
            <h3 class="section-header">⚡ Performance & Caching</h3>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'performance'" class="alert alert-success">
              Performance & Caching settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'performance'" class="alert alert-danger">
              {{ saveError }}
            </div>

            <!-- Save Button -->
            <div class="form-actions">
              <button @click="saveSection(['image_quality', 'cache_duration', 'max_image_width', 'max_image_height', 'enable_image_optimization', 'enable_cdn', 'enable_compression'], 'performance')" class="btn btn-primary" :disabled="saving">
                <span class="icon">💾</span>
                {{ saving ? 'Saving...' : 'Save Performance Settings' }}
              </button>
            </div>
          </div>

          <!-- System Settings Section -->
          <div class="content-section">
            <h3 class="section-header">🔧 System Settings</h3>
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

            <!-- Success/Error Messages -->
            <div v-if="sectionSaveSuccess === 'system'" class="alert alert-success">
              System settings saved successfully!
            </div>
            <div v-if="sectionSaveError === 'system'" class="alert alert-danger">
              {{ saveError }}
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
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import AdminLayout from '@/components/AdminLayout.vue'
import { useAuthStore } from '@/stores/auth'
import { logDebug, logError } from '@/services/logger'

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
const API_URL = 'http://localhost:5226/api/v1'

const activeTab = ref('general')
const settings = ref<SiteSetting[]>([])
const loading = ref(true)
const error = ref('')
const saving = ref(false)
const sectionSaveSuccess = ref<string | null>(null)
const sectionSaveError = ref<string | null>(null)
const saveError = ref('')
const testEmailAddress = ref('')
const testEmailSuccess = ref('')
const testEmailError = ref('')
const sendingTestEmail = ref(false)

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
const SETTING_METADATA: Record<string, { displayName: string; type: string; category: string; sortOrder: number }> = {
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
}

const handleToggleChange = (key: string, event: Event) => {
  const target = event.target as HTMLInputElement
  getSetting(key).value = target.checked ? 'true' : 'false'
}

const loadSettings = async () => {
  loading.value = true
  error.value = ''
  
  try {
    logDebug('Loading settings from admin API', { url: `${API_URL}/admin/settings` });
    
    const response = await fetch(`${API_URL}/admin/settings`, {
      headers: { 'Authorization': `Bearer ${authStore.token}` }
    })

    logDebug('Settings response received', { status: response.status });
    
    if (!response.ok) {
      const errorText = await response.text()
      logError('Settings load failed', new Error(errorText), { status: response.status });
      throw new Error(`Failed to load settings: ${response.status}`)
    }

    const data = await response.json()
    logDebug('Settings loaded successfully', { count: data.length });
    settings.value = data
    
    if (settings.value.length === 0) {
      error.value = 'No settings found in database. They should be seeded on startup.'
    }
  } catch (err: any) {
    logError('Failed to load settings', err);
    error.value = err.message || 'Failed to load settings'
  } finally {
    loading.value = false
  }
}

const saveSection = async (settingKeys: string[], sectionName: string) => {
  saving.value = true
  sectionSaveSuccess.value = null
  sectionSaveError.value = null
  saveError.value = ''
  
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
          body: JSON.stringify({ value: setting.value })
        })
      )
    }

    // Create missing settings via POST
    for (const key of missingKeys) {
      const meta = SETTING_METADATA[key]
      const currentValue = getSetting(key).value
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
            value: currentValue,
            type: meta?.type ?? 'Text',
            category: meta?.category ?? 'General',
            sortOrder: meta?.sortOrder ?? 0
          })
        })
      )
    }

    const results = await Promise.all(allPromises)
    
    logDebug('Save results received', { successful: results.filter(r => r.ok).length, failed: results.filter(r => !r.ok).length });
    
    const failedResults = results.filter(r => !r.ok)
    
    if (failedResults.length > 0) {
      const errorTexts = await Promise.all(failedResults.map(r => r.text()))
      logError('Some settings failed to save', new Error(`${failedResults.length} settings failed`), { errorTexts });
      throw new Error(`${failedResults.length} settings failed to save`)
    }

    sectionSaveSuccess.value = sectionName
    setTimeout(() => sectionSaveSuccess.value = null, 3000)
    
    // Reload settings to get updated values (including newly created IDs)
    await loadSettings()
  } catch (err: any) {
    logError('Failed to save settings', err);
    saveError.value = err.message || 'Failed to save settings'
    sectionSaveError.value = sectionName
    setTimeout(() => sectionSaveError.value = null, 5000)
  } finally {
    saving.value = false
  }
}

const sendTestEmail = async () => {
  if (!testEmailAddress.value) {
    testEmailError.value = 'Please enter an email address.'
    return
  }
  sendingTestEmail.value = true
  testEmailSuccess.value = ''
  testEmailError.value = ''
  try {
    const response = await fetch(`${API_URL}/admin/settings/test-email?email=${encodeURIComponent(testEmailAddress.value)}`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${authStore.token}` }
    })
    const data = await response.json()
    if (!response.ok) throw new Error(data.message || 'Failed to send test email')
    testEmailSuccess.value = data.message || 'Test email sent successfully!'
    setTimeout(() => testEmailSuccess.value = '', 5000)
  } catch (err: any) {
    testEmailError.value = err.message || 'Failed to send test email'
    setTimeout(() => testEmailError.value = '', 8000)
  } finally {
    sendingTestEmail.value = false
  }
}

const resetSettings = async () => {
  if (!confirm('Are you sure you want to reset all settings to defaults? This will overwrite all current values.')) {
    return
  }

  loading.value = true
  error.value = ''
  
  try {
    logDebug('Resetting settings to defaults');
    
    const response = await fetch(`${API_URL}/admin/settings/reset`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${authStore.token}`
      }
    })

    if (!response.ok) {
      const errorText = await response.text()
      logError('Reset settings failed', new Error(errorText));
      throw new Error('Failed to reset settings')
    }

    const result = await response.json()
    logDebug('Settings reset successful', result);
    
    // Reload settings
    await loadSettings()
    
    alert('Settings have been reset to defaults successfully!')
  } catch (err: any) {
    logError('Failed to reset settings', err);
    error.value = err.message || 'Failed to reset settings'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadSettings()
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

  .tabs {
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
  }

  .tab {
    padding: 0.75rem 1rem;
    white-space: nowrap;
  }
}
</style>