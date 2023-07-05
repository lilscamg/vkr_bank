const salt = window.crypto.getRandomValues(new Uint8Array(16));
const iv = window.crypto.getRandomValues(new Uint8Array(12));


// получить ключ
async function deriveKey(password, salt) {
    var utf8Encoder = new TextEncoder('utf-8');
    const buffer = utf8Encoder.encode(password);
    const key = await window.crypto.subtle.importKey(
        'raw',
        buffer,
        { name: 'PBKDF2' },
        false,
        ['deriveKey'],
    );

    var iterations = 25000
    
    const privateKey = window.crypto.subtle.deriveKey(
        {
            name: 'PBKDF2',
            hash: { name: 'SHA-256' },
            iterations,
            salt,
        },
        key,
        {
            name: 'AES-GCM',
            length: 256,
        },
        false,
        ['encrypt', 'decrypt'],
    );

    return privateKey;
}

// функции перевода base64 в массив байтов и обратно
const buff_to_base64 = (buff) => btoa(String.fromCharCode.apply(null, buff));
const base64_to_buf = (b64) => Uint8Array.from(atob(b64), (c) => c.charCodeAt(null));

// шифрование
export async function encrypt(key, data, iv, salt) {
    const buffer = new TextEncoder().encode(data);

    const privatekey = await deriveKey(key, salt);
    const encrypted = await window.crypto.subtle.encrypt(
        {
            name: 'AES-GCM',
            iv,
            tagLength: 128,
        },
        privatekey,
        buffer,
    );

    const bytes = new Uint8Array(encrypted);
    let buff = new Uint8Array(salt.byteLength + iv.byteLength + encrypted.byteLength);
    buff.set(salt, 0);
    buff.set(iv, salt.byteLength);
    buff.set(bytes, salt.byteLength + iv.byteLength);

    const base64Buff = buff_to_base64(buff);
    return base64Buff;
}

// дешифрование
export async function decrypt(key, data) {
    const d = base64_to_buf(data);
    const salt = d.slice(0, 16); // Fix 1: consider salt
    const iv = d.slice(16, 16 + 12)
    const ec = d.slice(16 + 12);

    const decrypted = await window.crypto.subtle.decrypt(
        {
            name: 'AES-GCM',
            iv,
            tagLength: 128,
        },
        await deriveKey(key, salt),
        ec
    );

    return new TextDecoder().decode(new Uint8Array(decrypted));
}


/* document.getElementById('_encrypt__message__button').addEventListener("click", () => {
    var message = document.getElementById('_encrypt__message__input').value
    var key = 'zhopa';
    var obj = {
        id: 1,
        name: "bulat",
        age: 30
    }
    console.log(obj)
    console.log(JSON.stringify(obj))
    console.log(typeof(JSON.stringify(obj)))

    console.log(`Message: ${message}`)
    encrypt(key, message, iv, salt).then((_ciphertext) => {

        console.log(`Ciphertext: ${_ciphertext}`)

        $.ajax({
            method: "GET",
            url: "/Main/DecryptMessageTest",
            data: {
                ciphertext: _ciphertext
            },
            success: function (response) {

            }
        })
    })
}) */