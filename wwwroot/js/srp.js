// генерация строки хэша SHA256
async function digestMessageString(message) {
    const msgUint8 = new TextEncoder().encode(message); // encode as (utf-8) Uint8Array
    const hashBuffer = await crypto.subtle.digest("SHA-256", msgUint8); // hash the message
    const hashArray = Array.from(new Uint8Array(hashBuffer)); // convert buffer to byte array
    const hashHex = hashArray
        .map((b) => b.toString(16).padStart(2, "0"))
        .join(""); // convert bytes to hex string
    return hashHex;
}
// генерация байтов хэша SHA256
async function digestMessageBytes(message) {
    const msgUint8 = new TextEncoder().encode(message); // encode as (utf-8) Uint8Array
    const hashBuffer = await crypto.subtle.digest("SHA-256", msgUint8); // hash the message
    const hashArray = Array.from(new Uint8Array(hashBuffer)); // convert buffer to byte array
    return hashArray;
}
// генерация соли
function getSalt() {
    const array = new Uint32Array(10);
    self.crypto.getRandomValues(array);
    return array[0].toString() + array[1].toString()
}
// рандом
function getRandom() {
    const array = new Uint32Array(10);
    self.crypto.getRandomValues(array);
    return array[0]
}
// из байтов в bigint
function byteArrayToBigInt(byteArray) {
    var value = 0n;
    for (var i = byteArray.length - 1; i >= 0; i--) {
        value = value * BigInt(256) + BigInt(byteArray[i]);
    }
    return BigInt(value);
};
// Mod
function Mod(x, m) {
    return (x % m + m) % m;
}
// to bin
function to_Bin(number) {
    var delimoe = BigInt(number);
    var result = "";
    while (true) {
        var chastnoe = delimoe / 2n;
        var ostatok = delimoe % 2n;
        delimoe = chastnoe;
        result += ostatok.toString();
        if (chastnoe == 0) {
            break;
        }
    }
    var charArray = result.split('');
    var charArrayRev = charArray.reverse();
    return charArrayRev.join("")
}
// ModPowFast
function ModPow(a, e, N) {
    var bin = to_Bin(e);
    const resArr = new BigInt64Array(bin.length)
    for (var i = 0; i < bin.length; i++) {
        if (i == 0) {
            resArr[i] = a;
            continue;
        }
        if (bin[i] == '0')
            resArr[i] = Mod(resArr[i - 1] * resArr[i - 1], N);
        if (bin[i] == '1')
            resArr[i] = Mod(resArr[i - 1] * resArr[i - 1] * a, N);
    }
    return resArr[resArr.length - 1];
}
// xor
function getXor(data1, data2) {
    var res = "";
    for (var i = 0; i < data1.length; i++) {
        res += ((parseInt(data1[i], 10) + parseInt(data2[i], 10)) % 2).toString()
    }
    return res;
}

// получение хэша с солью от пароля для регистрации srp
var passwordInput = document.getElementById('_reg__password__input')
passwordInput.addEventListener("change", (event) => {
    const g = 2n
    const N = 2096687n

    console.log("Регистрация:")
    var salt = getSalt()
    var password = passwordInput.value
    console.log("salt = " + salt)
    console.log("password = " + password)
    digestMessageBytes(salt.toString() + password.toString()).then((x_bytes) => {
        var x = byteArrayToBigInt(x_bytes)
        console.log("x = " + x.toString())
        var verifier = ModPow(g, x, N)
        console.log("verifier = " + verifier)
        document.getElementById('_srp__salt').value = salt
        document.getElementById('_srp__verifier').value = parseInt(verifier, 10)
    });
})

// авторизация srp, при вводе номера телефона 
var phoneInput = document.getElementById('_auth__phone__input')
phoneInput.addEventListener("change", (event) => {

    const g = 2n
    const N = 2096687n

    console.log("Авторизация, при вводе телефона:")
    var phoneNumber = phoneInput.value
    var a = BigInt(getRandom())
    console.log(`a = ${a}`)
    document.getElementById('_srp__a__auth').value = a
    var A = ModPow(g, a, N)
    console.log(`A = ${A}`)
    document.getElementById('_srp__A__auth').value = A


    $.ajax({
        method: "POST",
        url: "/Account/SRP_Auth_1",
        data:
        {
            "_phoneNumber": phoneNumber,
            "_A": A
        },
        success: function (response) {
            if (response == "") {
                alert("Ошибка! u = 0")
                return
            }
            const array = response.split(" ")
            var salt = array[0]
            var B = parseInt(array[1], 10)
            if (B == 0) {
                alert("B = 0")
                return
            }
            
            // console.log("Сгенерированный A: " + A)
            document.getElementById('_srp__salt__auth').value = salt
            // console.log("Полученный Salt: " + document.getElementById('_srp__salt__auth').value)
            document.getElementById('_srp__B__auth').value = B
            // console.log("Полученный B: " + document.getElementById('_srp__B__auth').value)
            // вычисление u
            digestMessageBytes(A.toString() + B.toString()).then((bytes) => {
                var u = byteArrayToBigInt(bytes)
                console.log("Scrambler client u = " + u)
                document.getElementById('_srp__u__auth').value = u
            });
        },
        error: function (response) {
            console.log(response)
        }
    })
})

// авторизация srp, при вводе пароля
var passwordInputAuth = document.getElementById('_auth__password__input')
passwordInputAuth.addEventListener("change", () => {
    const g = 2n
    const N = 2096687n
    const k = 3n

    var password = passwordInputAuth.value
    var salt = document.getElementById('_srp__salt__auth').value
    var B = document.getElementById('_srp__B__auth').value
    var a = document.getElementById('_srp__a__auth').value
    var u = document.getElementById('_srp__u__auth').value

    // вычисление ключа клиента
    digestMessageBytes(salt.toString() + password.toString()).then((bytes) => {
        console.log("Авторизация при вводе пароля:")
        var x = byteArrayToBigInt(bytes)
        console.log("x = ", x)
        var mul = k * ModPow(g, x, N)
        var dif = BigInt(B) - mul
        var exp = BigInt(a) + (BigInt(u) * x)
        var S_c = ModPow(dif, exp, N)
        console.log("S_c = ", S_c)
        var K_c;
        digestMessageBytes(S_c.toString()).then((bytes) => {
            K_c = byteArrayToBigInt(bytes)
            console.log("K_c = ", K_c)

            // хранение ключа в localstorage
            window.localStorage.setItem("K_c", K_c.toString())
            // console.log(`Из localstorage ${window.localStorage.getItem("K_c")}`)
        });

        // вычисление M_с
        console.log("Вычисление M на клиенте:")
        digestMessageBytes(N.toString()).then((hash_N_bytes) => {

            var hash_N_bin = to_Bin(byteArrayToBigInt(hash_N_bytes))
            // console.log("hash_N_bin: " + hash_N_bin)
            digestMessageBytes(g.toString()).then((hash_g_bytes) => {

                var hash_g_bin = to_Bin(byteArrayToBigInt(hash_g_bytes))
                // console.log("hash_g_bin: " + hash_g_bin)
                var xor = getXor(hash_N_bin, hash_g_bin);
                // console.log("xor: " + xor)
                var phoneNumber = document.getElementById('_auth__phone__input').value.toString()
                digestMessageBytes(phoneNumber).then((hash_I_bytes) => {

                    var hash_I = byteArrayToBigInt(hash_I_bytes).toString()
                    var A = document.getElementById('_srp__A__auth').value
                    var string = xor + hash_I + salt + A.toString() + B.toString() + K_c.toString()
                    // console.log("Строка: " + string)
                    digestMessageBytes(string).then((M_c_bytes) => {

                        const M_c = byteArrayToBigInt(M_c_bytes).toString()
                        console.log("M_c: " + M_c)

                        // отправка M на сервер
                        $.ajax({
                            method: "POST",
                            url: "/Account/SRP_Auth_2",
                            data:
                            {
                                "M_c": M_c
                            },
                            success: function (response) {
                                if (response.status == 0) {
                                    alert("Соединение разорвано");
                                    return
                                }
                                // вычисление R_с
                                var string = A.toString() + M_c + K_c.toString()
                                digestMessageBytes(string).then((R_c_bytes) => {
                                    var R_c = byteArrayToBigInt(R_c_bytes)
                                    console.log("R_c = ", R_c.toString())
                                    document.getElementById('_srp__R_c__auth').value = R_c.toString()

                                    // проверка на 2fa
                                    if (response._2fa != 0) {
                                        alert("Введите код двухфакторной авторизации")
                                        document.getElementById('_2fa__block').style.display = 'block'
                                    }
                                });
                            },
                            error: function (response) {
                                console.log(response)
                            }
                        })
                    });
                });
            });
        });
    });
})