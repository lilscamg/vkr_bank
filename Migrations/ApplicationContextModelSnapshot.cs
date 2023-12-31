﻿// <auto-generated />
using System;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace vkr_bank.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    partial class ApplicationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("vkr_bank.Models.Credit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ApplicationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ApprovalDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double>("CreditAmount")
                        .HasColumnType("double precision");

                    b.Property<double>("CreditBalance")
                        .HasColumnType("double precision");

                    b.Property<int>("CreditTerm")
                        .HasColumnType("integer");

                    b.Property<int>("CreditType")
                        .HasColumnType("integer");

                    b.Property<double>("DebtAmount")
                        .HasColumnType("double precision");

                    b.Property<double>("MonthlyPayment")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("NextPaymentDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("NumberOfPayments")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("StatusMessage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<bool>("isDifferentiated")
                        .HasColumnType("boolean");

                    b.Property<bool>("isOverdue")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("Credits");
                });

            modelBuilder.Entity("vkr_bank.Models.CreditProccessing", b =>
                {
                    b.Property<int>("CreditId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CreditId"));

                    b.Property<string>("request_cr_str")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("request_oi_str")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("request_ui_str")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("CreditId");

                    b.ToTable("CreditProccessings");
                });

            modelBuilder.Entity("vkr_bank.Models.EmploymentRegister", b =>
                {
                    b.Property<int>("RecordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RecordId"));

                    b.Property<DateTime>("BeginningOfWork")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("OrganizationId")
                        .HasColumnType("integer");

                    b.Property<double>("Salary")
                        .HasColumnType("double precision");

                    b.Property<string>("UserPassport")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("RecordId");

                    b.ToTable("EmploymentRegisters");
                });

            modelBuilder.Entity("vkr_bank.Models.Organization", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Organizations");
                });

            modelBuilder.Entity("vkr_bank.Models.OrganizationInfo", b =>
                {
                    b.Property<int>("RecordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RecordId"));

                    b.Property<int>("OrganizationId")
                        .HasColumnType("integer");

                    b.Property<double>("Salary")
                        .HasColumnType("double precision");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("RecordId");

                    b.ToTable("OrganizationInfos");
                });

            modelBuilder.Entity("vkr_bank.Models.UserInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BirthTime")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Passport")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SecondName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ThirdName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("hasCar")
                        .HasColumnType("boolean");

                    b.Property<bool>("hasChildren")
                        .HasColumnType("boolean");

                    b.Property<bool>("hasCreditInAnotherBank")
                        .HasColumnType("boolean");

                    b.Property<bool>("hasFamily")
                        .HasColumnType("boolean");

                    b.Property<bool>("hasHigherEducation")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("UserInfos");
                });

            modelBuilder.Entity("vkr_bank.Models.UserSRP", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("BanDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("LoginAttempts")
                        .HasColumnType("integer");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<BigInteger>("Verifier")
                        .HasColumnType("numeric");

                    b.Property<int>("_2fa")
                        .HasColumnType("integer");

                    b.Property<string>("emailCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("emailCodeDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("UserSRPs");
                });
#pragma warning restore 612, 618
        }
    }
}
