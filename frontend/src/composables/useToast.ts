import { ref } from 'vue';

export type ToastType = 'info' | 'success' | 'warning' | 'error';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
}

const toasts = ref<Toast[]>([]);
let nextId = 0;

export function useToast() {
  const show = (message: string, type: ToastType = 'info', duration = 5000) => {
    const id = ++nextId;
    toasts.value.push({ id, message, type });
    setTimeout(() => {
      toasts.value = toasts.value.filter(t => t.id !== id);
    }, duration);
  };

  return {
    toasts,
    show,
    info:    (msg: string) => show(msg, 'info'),
    success: (msg: string) => show(msg, 'success'),
    warning: (msg: string) => show(msg, 'warning'),
    error:   (msg: string) => show(msg, 'error'),
  };
}
