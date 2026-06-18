// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => { });

self.addEventListener('push', event => {
    const data = event.data ? event.data.json() : {};

    event.waitUntil(
        self.registration.showNotification(data.title || 'Novo agendamento', {
            body: data.body || 'Voce recebeu um novo agendamento.',
            icon: 'icon-192.png',
            badge: 'icon-192.png',
            data: { url: data.url || '/' }
        })
    );
});

self.addEventListener('notificationclick', event => {
    event.notification.close();

    const targetUrl = new URL(event.notification.data?.url || '/', self.location.origin).href;

    event.waitUntil((async () => {
        const clientList = await clients.matchAll({ type: 'window', includeUncontrolled: true });
        const client = clientList.find(item => item.url === targetUrl);

        if (client) {
            await client.focus();
            return;
        }

        await clients.openWindow(targetUrl);
    })());
});
