import React from 'react';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';

export default function Home() {
  const { siteConfig } = useDocusaurusContext();
  return (
    <Layout
      title={`Hello from ${siteConfig.title}`}
      description="Description will go into a meta tag in <head />">
      <main>
        <div className="container">
          <Link
            className="button button--secondary button--lg"
            target="_blank"
            to="/wasm/yuremono.html">
            zig wasm ⚡
          </Link>
        </div>
      </main>
    </Layout>
  );
};
