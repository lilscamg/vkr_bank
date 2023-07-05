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
async function encrypt(key, data, iv, salt) {
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
async function decrypt(key, data) {
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

// скролл
document.addEventListener("DOMContentLoaded", (event) => {
    var button = document.getElementById('_credit__button');
    var credit_apply = document.getElementById("_credit__apply");
    if (credit_apply != null) {
        button.addEventListener('click', function () {
            credit_apply.scrollIntoView({ behavior: "smooth" });
        });
    }
});

// маска телефона
document.addEventListener("DOMContentLoaded", (event) => {
    var phone_inputs = document.querySelectorAll('.phone_input');
    var form_btns = document.querySelectorAll("._form__send")
    var phone_masks = []
    for (let i = 0; i < phone_inputs.length; i++) {
        phone_masks.push(new IMask(phone_inputs[i], {
            mask: '+{7} (000) 000-00-00'
        }));
        phone_inputs[i].addEventListener("input",
            function () { phone_input_handler(phone_masks[i], form_btns[i]) })

    }
    function phone_input_handler(mask, form_btn) {
        if (mask.masked.isComplete) {
            form_btn.classList.add("_form__send-active")
        }
        else {
            form_btn.classList.remove("_form__send-active")
        }
    }
});

// маска паспорта
document.addEventListener("DOMContentLoaded", (event) => {
    var passport_inputs = document.querySelectorAll('[id=_user__info__pas]');
    var passport_masks = []
    for (let i = 0; i < passport_inputs.length; i++) {
        passport_masks.push(new IMask(passport_inputs[i], {
            mask: '0000 000000'
        }))
    }
});

// место работы
var organization_input = document.getElementById('_organization__name__input')
var organizations_suggestions = document.getElementById('_organization__name__suggestions')
if (organization_input != null) {
    organization_input.addEventListener("click", () => {
        $.ajax({
            method: "GET",
            url: "/Credit/GetAllOrganizations",
            success: function (response) {
                organizations_suggestions.innerHTML = ""
                for (var i = 0; i < response.length; i++) {
                    // формирование блока
                    const row = document.createElement("div")
                    row.className = "_organization__name__suggestions__row"
                    row.innerHTML = response[i].name
                    // добавление события
                    row.addEventListener("click", () => {
                        OrganizationChoice(row)
                    })
                    // вставка строки организцаии в блок
                    organizations_suggestions.appendChild(row)
                }
            }
        })
    })
    organization_input.addEventListener("input", function () {
        var _key = organization_input.value
        if (_key.length == 0) {
            $.ajax({
                method: "GET",
                url: "/Credit/GetAllOrganizations",
                success: function (response) {
                    organizations_suggestions.innerHTML = ""
                    for (var i = 0; i < response.length; i++) {
                        // формирование блока
                        const row = document.createElement("div")
                        row.className = "_organization__name__suggestions__row"
                        row.innerHTML = response[i].name
                        // добавление события
                        row.addEventListener("click", () => {
                            OrganizationChoice(row)
                        })
                        // вставка строки организцаии в блок
                        organizations_suggestions.appendChild(row)
                    }
                }
            })
        }
        if (_key != null && _key.length != 0) {
            $.ajax({
                method: "GET",
                url: "/Credit/GetOrganizations",
                data: {
                    key: _key
                },
                success: function (response) {
                    organizations_suggestions.innerHTML = ""
                    for (var i = 0; i < response.length; i++) {
                        // формирование блока
                        const row = document.createElement("div")
                        row.className = "_organization__name__suggestions__row"
                        row.innerHTML = response[i].name
                        // добавление события
                        row.addEventListener("click", () => {
                            OrganizationChoice(row)
                        })
                        // вставка строки организцаии в блок
                        organizations_suggestions.appendChild(row)
                    }
                }
            })
        }
    })
    organization_input.addEventListener("change", function () {
        organizations_suggestions.innerHTML = ""
    })
}

// кредит
document.addEventListener("DOMContentLoaded", (event) => {
    // кредитная информация
    let credit_amount_number = document.getElementById('_credit__amount__number')
    let credit_amount_range = document.getElementById('_credit__amount__range')
    let credit_term_number = document.getElementById('_credit__term__number')
    let credit_term_range = document.getElementById('_credit__term__range')
    let credit_monthly_value = document.getElementById('_monthly__payment')
    let credit_type = document.getElementById('_credit__select')
    const potreb = 14.9
    const ipoteka = 9.34
    const auto = 5.9
    let stavka

    // ввод суммы кредита
    if (credit_amount_number != null) {
        credit_amount_number.addEventListener("input", function () {
            // изменение range
            credit_amount_range.value = credit_amount_number.value
            // определение кредитной ставки
            if (credit_type.value == 1)
                stavka = potreb
            if (credit_type.value == 2)
                stavka = ipoteka
            if (credit_type.value == 3)
                stavka = auto
            // переменные
            let amount = parseInt(credit_amount_number.value)
            let term = parseInt(credit_term_number.value)
            // расчет платежа
            const radioButtons = document.querySelectorAll('input[name="_payment__type"]');
            for (const radioButton of radioButtons) {
                if (radioButton.checked) {
                    if (radioButton.value == 0) {
                        let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                        credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                        document.getElementById('_monthly__payment__form').value = credit_amount_payment
                    }
                    else {
                        var b = amount / term
                        var Sn = amount - (b * 0)
                        var p = Sn * ((stavka / 100) / 12)
                        let credit_amount_payment = Math.round((b + p) * 100) / 100
                        credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                        document.getElementById('_monthly__payment__form').value = credit_amount_payment
                    }
                }
            }
            // изменение переплаты
            for (const radioButton of radioButtons) {
                if (radioButton.checked) {
                    if (radioButton.value == 0) {
                        let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                        let overpayment = credit_amount_payment * term - amount
                        overpayment = Math.round(overpayment * 100) / 100
                        document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                    }
                    else {
                        // переменные
                        let overpayment = 0
                        let numberOfPayments = 0
                        // подсчет переплаты
                        for (var i = 1; i <= term; i++) {
                            let b = amount / term;
                            let Sn = amount - (b * numberOfPayments);
                            let p = Sn * ((stavka / 100) / 12);
                            overpayment += (b + p);
                            numberOfPayments += 1
                        }
                        overpayment = Math.round((overpayment - amount) * 100) / 100
                        document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                    }
                }
            }
        })
    }
    if (credit_amount_range != null) {
        credit_amount_range.addEventListener("input", function () {
            // изменение input
            credit_amount_number.value = credit_amount_range.value
            // определение кредитной ставки
            if (credit_type.value == 1)
                stavka = potreb
            if (credit_type.value == 2)
                stavka = ipoteka
            if (credit_type.value == 3)
                stavka = auto
            // переменные
            let amount = parseInt(credit_amount_number.value)
            let term = parseInt(credit_term_number.value)
            // расчет платежа
            const radioButtons = document.querySelectorAll('input[name="_payment__type"]');
            for (const radioButton of radioButtons) {
                if (radioButton.checked) {
                    if (radioButton.value == 0) {
                        let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                        credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                        document.getElementById('_monthly__payment__form').value = credit_amount_payment
                    }
                    else {
                        var b = amount / term
                        var Sn = amount - (b * 0)
                        var p = Sn * ((stavka / 100) / 12)
                        let credit_amount_payment = Math.round((b + p) * 100) / 100
                        credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                        document.getElementById('_monthly__payment__form').value = credit_amount_payment
                    }
                }
            }
            // изменение переплаты
            for (const radioButton of radioButtons) {
                if (radioButton.checked) {
                    if (radioButton.value == 0) {
                        let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                        let overpayment = credit_amount_payment * term - amount
                        overpayment = Math.round(overpayment * 100) / 100
                        document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                    }
                    else {
                        // переменные
                        let overpayment = 0
                        let credit_balance = 0
                        let numberOfPayments = 0
                        // подсчет переплаты
                        for (var i = 1; i <= term; i++) {
                            let b = amount / term;
                            let Sn = amount - (b * numberOfPayments);
                            let p = Sn * ((stavka / 100) / 12);
                            overpayment += (b + p);
                            credit_balance = credit_balance - b;
                            numberOfPayments += 1
                        }
                        overpayment = Math.round((overpayment - amount) * 100) / 100
                        document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                    }
                }
            }
        })
    }

    // ввод срока кредита
    if (credit_term_number != null) {
        credit_term_number.addEventListener("input", function () {
            // изменение range
            credit_term_range.value = credit_term_number.value
            // определение кредитной ставки
            if (credit_type.value == 1)
                stavka = potreb
            if (credit_type.value == 2)
                stavka = ipoteka
            if (credit_type.value == 3)
                stavka = auto
            // переменные
            let amount = parseInt(credit_amount_number.value)
            let term = parseInt(credit_term_number.value)
            // расчет платежа
            const radioButtons = document.querySelectorAll('input[name="_payment__type"]');
            for (const radioButton of radioButtons) {
                if (radioButton.checked) {
                    if (radioButton.value == 0) {
                        let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                        credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                        document.getElementById('_monthly__payment__form').value = credit_amount_payment
                    }
                    else {
                        var b = amount / term
                        var Sn = amount - (b * 0)
                        var p = Sn * ((stavka / 100) / 12)
                        let credit_amount_payment = Math.round((b + p) * 100) / 100
                        credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                        document.getElementById('_monthly__payment__form').value = credit_amount_payment
                    }
                }
            }
            // изменение переплаты
            for (const radioButton of radioButtons) {
                if (radioButton.checked) {
                    if (radioButton.value == 0) {
                        let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                        let overpayment = credit_amount_payment * term - amount
                        overpayment = Math.round(overpayment * 100) / 100
                        document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                    }
                    else {
                        // переменные
                        let overpayment = 0
                        let credit_balance = 0
                        let numberOfPayments = 0
                        // подсчет переплаты
                        for (var i = 1; i <= term; i++) {
                            let b = amount / term;
                            let Sn = amount - (b * numberOfPayments);
                            let p = Sn * ((stavka / 100) / 12);
                            overpayment += (b + p);
                            credit_balance = credit_balance - b;
                            numberOfPayments += 1
                        }
                        overpayment = Math.round((overpayment - amount) * 100) / 100
                        document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                    }
                }
            }
        })
    }
    if (credit_term_range != null) {
        credit_term_range.addEventListener("input", function () {
            // изменение range
            credit_term_number.value = credit_term_range.value
            // определение кредитной ставки
            if (credit_type.value == 1)
                stavka = potreb
            if (credit_type.value == 2)
                stavka = ipoteka
            if (credit_type.value == 3)
                stavka = auto
            // переменные
            let amount = parseInt(credit_amount_number.value)
            let term = parseInt(credit_term_number.value)
            // расчет платежа
            const radioButtons = document.querySelectorAll('input[name="_payment__type"]');
            for (const radioButton of radioButtons) {
                if (radioButton.checked) {
                    if (radioButton.value == 0) {
                        let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                        credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                        document.getElementById('_monthly__payment__form').value = credit_amount_payment
                    }
                    else {
                        var b = amount / term
                        var Sn = amount - (b * 0)
                        var p = Sn * ((stavka / 100) / 12)
                        let credit_amount_payment = Math.round((b + p) * 100) / 100
                        credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                        document.getElementById('_monthly__payment__form').value = credit_amount_payment
                    }
                }
            }
            // изменение переплаты
            for (const radioButton of radioButtons) {
                if (radioButton.checked) {
                    if (radioButton.value == 0) {
                        let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                        let overpayment = credit_amount_payment * term - amount
                        overpayment = Math.round(overpayment * 100) / 100
                        document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                    }
                    else {
                        // переменные
                        let overpayment = 0
                        let credit_balance = 0
                        let numberOfPayments = 0
                        // подсчет переплаты
                        for (var i = 1; i <= term; i++) {
                            let b = amount / term;
                            let Sn = amount - (b * numberOfPayments);
                            let p = Sn * ((stavka / 100) / 12);
                            overpayment += (b + p);
                            credit_balance = credit_balance - b;
                            numberOfPayments += 1
                        }
                        overpayment = Math.round((overpayment - amount) * 100) / 100
                        document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                    }
                }
            }
        })
    }

    // изменение процентной ставки
    if (credit_type != null) {
        credit_type.addEventListener("change", (event) => {
            if (credit_type.value == 1) {
                document.querySelector("._credit__interest__rate__value").innerHTML = potreb.toString() + " %"
                stavka = potreb
                let amount = parseInt(credit_amount_number.value)
                let term = parseInt(credit_term_number.value)
                // расчет платежа
                const radioButtons = document.querySelectorAll('input[name="_payment__type"]');
                for (const radioButton of radioButtons) {
                    if (radioButton.checked) {
                        if (radioButton.value == 0) {
                            let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                            credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                            document.getElementById('_monthly__payment__form').value = credit_amount_payment
                        }
                        else {
                            var b = amount / term
                            var Sn = amount - (b * 0)
                            var p = Sn * ((stavka / 100) / 12)
                            let credit_amount_payment = Math.round((b + p) * 100) / 100
                            credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                            document.getElementById('_monthly__payment__form').value = credit_amount_payment
                        }
                    }
                }
                // изменение переплаты
                for (const radioButton of radioButtons) {
                    if (radioButton.checked) {
                        if (radioButton.value == 0) {
                            let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                            let overpayment = credit_amount_payment * term - amount
                            overpayment = Math.round(overpayment * 100) / 100
                            document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                        }
                        else {
                            // переменные
                            let overpayment = 0
                            let credit_balance = 0
                            let numberOfPayments = 0
                            // подсчет переплаты
                            for (var i = 1; i <= term; i++) {
                                let b = amount / term;
                                let Sn = amount - (b * numberOfPayments);
                                let p = Sn * ((stavka / 100) / 12);
                                overpayment += (b + p);
                                credit_balance = credit_balance - b;
                                numberOfPayments += 1
                            }
                            overpayment = Math.round((overpayment - amount) * 100) / 100
                            document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                        }
                    }
                }
            }
            if (credit_type.value == 2) {
                document.querySelector("._credit__interest__rate__value").innerHTML = ipoteka.toString() + " %"
                stavka = ipoteka
                let amount = parseInt(credit_amount_number.value)
                let term = parseInt(credit_term_number.value)
                // расчет платежа
                const radioButtons = document.querySelectorAll('input[name="_payment__type"]');
                for (const radioButton of radioButtons) {
                    if (radioButton.checked) {
                        if (radioButton.value == 0) {
                            let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                            credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                            document.getElementById('_monthly__payment__form').value = credit_amount_payment
                        }
                        else {
                            var b = amount / term
                            var Sn = amount - (b * 0)
                            var p = Sn * ((stavka / 100) / 12)
                            let credit_amount_payment = Math.round((b + p) * 100) / 100
                            credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                            document.getElementById('_monthly__payment__form').value = credit_amount_payment
                        }
                    }
                }
                // изменение переплаты
                for (const radioButton of radioButtons) {
                    if (radioButton.checked) {
                        if (radioButton.value == 0) {
                            let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                            let overpayment = credit_amount_payment * term - amount
                            overpayment = Math.round(overpayment * 100) / 100
                            document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                        }
                        else {
                            // переменные
                            let overpayment = 0
                            let credit_balance = 0
                            let numberOfPayments = 0
                            // подсчет переплаты
                            for (var i = 1; i <= term; i++) {
                                let b = amount / term;
                                let Sn = amount - (b * numberOfPayments);
                                let p = Sn * ((stavka / 100) / 12);
                                overpayment += (b + p);
                                credit_balance = credit_balance - b;
                                numberOfPayments += 1
                            }
                            overpayment = Math.round((overpayment - amount) * 100) / 100
                            document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                        }
                    }
                }
            }
            if (credit_type.value == 3) {
                document.querySelector("._credit__interest__rate__value").innerHTML = auto.toString() + " %"
                stavka = auto
                let amount = parseInt(credit_amount_number.value)
                let term = parseInt(credit_term_number.value)
                // расчет платежа
                const radioButtons = document.querySelectorAll('input[name="_payment__type"]');
                for (const radioButton of radioButtons) {
                    if (radioButton.checked) {
                        if (radioButton.value == 0) {
                            let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                            credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                            document.getElementById('_monthly__payment__form').value = credit_amount_payment
                        }
                        else {
                            var b = amount / term
                            var Sn = amount - (b * 0)
                            var p = Sn * ((stavka / 100) / 12)
                            let credit_amount_payment = Math.round((b + p) * 100) / 100
                            credit_monthly_value.innerHTML = new Intl.NumberFormat('ru-RU').format(credit_amount_payment) + " ₽"
                            document.getElementById('_monthly__payment__form').value = credit_amount_payment
                        }
                    }
                }
                // изменение переплаты
                for (const radioButton of radioButtons) {
                    if (radioButton.checked) {
                        if (radioButton.value == 0) {
                            let credit_amount_payment = Math.round((amount * (stavka / (100 * 12)) / (1 - Math.pow(1 + (stavka / (100 * 12)), (-1) * term))) * 100) / 100
                            let overpayment = credit_amount_payment * term - amount
                            overpayment = Math.round(overpayment * 100) / 100
                            document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                        }
                        else {
                            // переменные
                            let overpayment = 0
                            let credit_balance = 0
                            let numberOfPayments = 0
                            // подсчет переплаты
                            for (var i = 1; i <= term; i++) {
                                let b = amount / term;
                                let Sn = amount - (b * numberOfPayments);
                                let p = Sn * ((stavka / 100) / 12);
                                overpayment += (b + p);
                                credit_balance = credit_balance - b;
                                numberOfPayments += 1
                            }
                            overpayment = Math.round((overpayment - amount) * 100) / 100
                            document.getElementById('_overpayment').innerHTML = new Intl.NumberFormat('ru-RU').format(overpayment) + " ₽"
                        }
                    }
                }
            }
        })
    }

    // подача заявки на кредит (нажатие на кнопку)
    let credit_submit = document.getElementById('_credit__submit__btn')
    if (credit_submit != null) {
        credit_submit.addEventListener('click', function () {
            // получение объектов
            var user_info = {
                Id: document.getElementById('_user__info__id').value,
                SecondName: document.getElementById('_user__info__sn').value,
                FirstName: document.getElementById('_user__info__fn').value,
                ThirdName: document.getElementById('_user__info__tn').value,
                BirthTime: document.getElementById('_user__info__bt').value,
                Email: document.getElementById('_user__info__em').value,
                Passport: document.getElementById('_user__info__pas').value
            }
            var organization_info = {
                UserId: user_info.Id,
                OrganizationName: document.getElementById('_organization__name__input').value,
                Salary: document.getElementById('_salary__input').value
            }
            var _isDifferentiated = true
            if (document.getElementById('_payment__type').value == 0) {
                _isDifferentiated = false
            }
            var credit = {
                UserId: user_info.Id,
                CreditType: document.getElementById('_credit__select').value,
                CreditAmount: document.getElementById('_credit__amount__number').value,
                CreditTerm: document.getElementById('_credit__term__number').value,
                MonthlyPayment: document.getElementById('_monthly__payment__form').value,
                isDifferentiated: _isDifferentiated
            }
            console.log(credit.MonthlyPayment)
            // валидация суммы кредита
            if (credit.CreditAmount < 20000 || credit.CreditAmount > 5000000) {
                document.getElementById('_tip__amount').innerHTML = "Минимальная сумма кредита - 20000 ₽, максмальная - 5000000 ₽"
                return
            }
            else {
                document.getElementById('_tip__amount').innerHTML = ""
            }
            // валидация срока кредита
            if (credit.CreditTerm < 12 || credit.CreditTerm > 60) {
                document.getElementById('_tip__term').innerHTML = "Минимальный срок кредита - 12 мес., максимальный - 60 мес."
                return
            }
            else {
                document.getElementById('_tip__term').innerHTML = ""
            }
            // валидация organization_info
            if (organization_info.OrganizationName == "" || organization_info.Salary == "") {
                alert("Заполните сведения о месте работы")
                return
            }
            // валидация user_info
            if (user_info.SecondName == "" || user_info.FirstName == "" || user_info.BirthTime == "" || user_info.Passport == "") {
                alert("Заполните персональную информацию")
                return
            }
            // шифрование данных
            var K_c = window.localStorage.getItem("K_c")
            var salt = window.crypto.getRandomValues(new Uint8Array(16));
            var iv = window.crypto.getRandomValues(new Uint8Array(12));
            encrypt(K_c, JSON.stringify(user_info), iv, salt).then((user_info_enc) => {
                encrypt(K_c, JSON.stringify(credit), iv, salt).then((credit_enc) => {
                    encrypt(K_c, JSON.stringify(organization_info), iv, salt).then((organization_info_enc) => {
                        // отправка формы
                        $.ajax({
                            method: "POST",
                            url: "/Credit/CreditApply",
                            data: {
                                ui_enc: user_info_enc,
                                cr_enc: credit_enc,
                                oi_enc: organization_info_enc
                            },
                            success: function () {
                                alert('Заявка на кредит отправлена. За ее статусом следите в личном кабинете.')
                                window.location.href = "https://localhost:7163"
                            }
                        })
                    })
                })
            })
        })
    }
});

// свернуть график платежей
let schedule__btn__down = document.querySelector("._credit__info__payment__schedule__btn__down")
let schedule__btn__up = document.querySelector("._credit__info__payment__schedule__btn__up")
let schedule__list = document.querySelector("._credit__info__payment__schedule__list")
if (schedule__btn__down != null) {
    schedule__btn__down.addEventListener("click", () => {
        schedule__btn__up.style.display = "block"
        schedule__btn__down.style.display = "none"
        schedule__list.style.display = "block"
    })
}
if (schedule__btn__up != null) {
    schedule__btn__up.addEventListener("click", () => {
        schedule__btn__down.style.display = "block"
        schedule__btn__up.style.display = "none"
        schedule__list.style.display = "none"
    })
}

// оплата кредита кнопка
var make_payment_btn = document.getElementById('_credit__make__payment__button')
if (make_payment_btn != null) {
    document.getElementById('_credit__make__payment__button').addEventListener("click", () => {
        var params_obj = {
            CreditId: document.getElementById('_credit__id').value,
            PaymentAmount: document.getElementById('_credit__make__payment__input').value
        }
        console.log(params_obj)
        // шифрование
        var K_c = window.localStorage.getItem("K_c")
        var salt = window.crypto.getRandomValues(new Uint8Array(16));
        var iv = window.crypto.getRandomValues(new Uint8Array(12));

        encrypt(K_c, JSON.stringify(params_obj), iv, salt).then((params_obj_string_enc) => {
            $.ajax({
                method: "POST",
                url: "/Credit/MakePayment",
                data: {
                    enc_string_json: params_obj_string_enc
                },
                success: function (response) {
                    if (response) {
                        alert("Платеж внесен")
                        window.location.reload()
                    }
                    else {
                        alert("Сумма платежа недостаточна для оплаты")
                    }
                },
                error: function (response) {
                    alert("Произошла ошибка")
                }
            })
        })
    })
}

// шифрование 2fa кода
var _2fa_input = document.getElementById('_auth__2fa__input')
if (_2fa_input != null) {
    _2fa_input.addEventListener("change", () => {
        // шифрование
        var K_c = window.localStorage.getItem("K_c")
        var salt = window.crypto.getRandomValues(new Uint8Array(16));
        var iv = window.crypto.getRandomValues(new Uint8Array(12));

        encrypt(K_c, _2fa_input.value.toString(), iv, salt).then((_2fa_enc) => {
            console.log(_2fa_enc)
            document.getElementById('_2fa__enc__code').value = _2fa_enc
        })
    })
}