import api from './api'

export type SettingsMap = Record<string, string>

const settingsService = {
  async getAll(): Promise<SettingsMap> {
    const { data } = await api.get('/api/settings/secure')
    return data
  },

  async bulkUpdate(updates: SettingsMap): Promise<void> {
    await api.put('/api/settings', updates)
  },

  async updateOne(key: string, value: string): Promise<void> {
    await api.put(`/api/settings/${key}`, { value })
  },
}

export default settingsService
