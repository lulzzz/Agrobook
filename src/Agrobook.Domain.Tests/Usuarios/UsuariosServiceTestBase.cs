﻿using Agrobook.Domain.Tests.Utils;
using Agrobook.Domain.Usuarios;
using Agrobook.Infrastructure.Cryptography;
using Agrobook.Infrastructure.Serialization;

namespace Agrobook.Domain.Tests.Usuarios
{
    public abstract class UsuariosServiceTestBase
    {
        protected TestableEventSourcedService<UsuariosService> sut;
        protected CryptoSerializer crypto;

        public UsuariosServiceTestBase()
        {
            this.crypto = new CryptoSerializer(new FauxCrypto());
            this.sut = new TestableEventSourcedService<UsuariosService>(
                r => new UsuariosService(r, new SimpleDateTimeProvider(), this.crypto));
        }
    }
}